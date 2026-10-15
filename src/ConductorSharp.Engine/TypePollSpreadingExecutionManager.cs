using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Client.Util;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Polling;
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine
{
    internal class TypePollSpreadingExecutionManager : IExecutionManager
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly WorkerSetConfig _configuration;
        private readonly ILogger<ExecutionManager> _logger;
        private readonly ITaskService _taskManager;
        private readonly IEnumerable<TaskToWorker> _registeredWorkers;
        private readonly IPollTimingStrategy _pollTimingStrategy;
        private readonly IPollOrderStrategy _pollOrderStrategy;
        private readonly TaskQueuePollingService _taskQueuePollingService;
        private readonly PolledTaskRunner _runner;

        public TypePollSpreadingExecutionManager(
            WorkerSetConfig options,
            ILogger<ExecutionManager> logger,
            ITaskService taskService,
            IEnumerable<TaskToWorker> workerMappings,
            IPollTimingStrategy pollTimingStrategy,
            IPollOrderStrategy pollOrderStrategy,
            TaskQueuePollingService taskQueuePollingService,
            PolledTaskRunner runner
        )
        {
            _configuration = options;
            _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentWorkers);
            _logger = logger;
            _taskManager = taskService;
            _registeredWorkers = workerMappings;
            _pollTimingStrategy = pollTimingStrategy;
            _pollOrderStrategy = pollOrderStrategy;
            _taskQueuePollingService = taskQueuePollingService;
            _runner = runner;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var currentSleepInterval = _configuration.SleepInterval;

            while (!cancellationToken.IsCancellationRequested)
            {
                var queuedTasks = (await _taskQueuePollingService.ListQueuesAsync(cancellationToken))
                    .Where(a => a.Value > 0)
                    .ToDictionary(a => a.Key, a => a.Value);

                var scheduledWorkers = _registeredWorkers.Where(a => queuedTasks.ContainsKey(GetQueueTaskName(a))).ToList();

                currentSleepInterval = _pollTimingStrategy.CalculateDelay(
                    queuedTasks,
                    scheduledWorkers,
                    _configuration.SleepInterval,
                    currentSleepInterval
                );

                scheduledWorkers = _pollOrderStrategy.CalculateOrder(queuedTasks, scheduledWorkers, _semaphore.CurrentCount);

                foreach (var scheduledWorker in scheduledWorkers)
                {
                    var queueName = GetQueueTaskName(scheduledWorker);
                    if (!queuedTasks.TryGetValue(queueName, out var queueDepth))
                        continue;

                    var pollsToLaunch = Math.Min(queueDepth, _semaphore.CurrentCount);

                    for (var i = 0; i < pollsToLaunch; i++)
                    {
                        await _semaphore.WaitAsync(cancellationToken);
                        _ = PollAndHandle(scheduledWorker, cancellationToken).ContinueWith(_ => _semaphore.Release());
                    }
                }

                await Task.Delay(currentSleepInterval, cancellationToken);
            }
        }

        private string GetQueueTaskName(TaskToWorker taskToWorker)
        {
            if (taskToWorker.TaskDomain != null)
                return $"{taskToWorker.TaskDomain}:{taskToWorker.TaskName}";
            if (_configuration.Domain != null)
                return $"{_configuration.Domain}:{taskToWorker.TaskName}";

            return taskToWorker.TaskName;
        }

        private async Task PollAndHandle(TaskToWorker scheduledWorker, CancellationToken cancellationToken)
        {
            Client.Generated.Task pollResponse;

            // TODO: Maybe this should be configurable
            var workerId = Guid.NewGuid().ToString();
            try
            {
                pollResponse = await _taskManager.PollAsync(
                    scheduledWorker.TaskName,
                    workerId,
                    scheduledWorker.TaskDomain ?? _configuration.Domain,
                    cancellationToken
                );
            }
            catch (ApiException exception) when (exception.StatusCode == 204)
            {
                // This handles the case when PollAsync throws exception in case there are no tasks in queue
                // Even though Conductor reports 1 task in queue for particular task type this endpoint won't return scheduled task immmediately
                // We skip the further handling as task will be handled in next call to this method
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception during the task polling");
                return;
            }

            // A transport other than HTTP may answer "no task" with null instead of a 204, as
            // Maestro's in-process transport does. Handling it here keeps that from surfacing as a
            // NullReferenceException in a poll nobody awaits.
            if (pollResponse == null)
                return;

            await _runner.RunAsync(pollResponse, workerId, scheduledWorker, cancellationToken);
        }
    }
}
