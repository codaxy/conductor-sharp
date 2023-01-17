using ConductorSharp.Client;
using ConductorSharp.Client.Model.Response;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Polling;

namespace ConductorSharp.Engine
{
    internal class ExecutionManager
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly WorkerSetConfig _configuration;
        private readonly ILogger<ExecutionManager> _logger;
        private readonly ITaskService _taskManager;
        private readonly IEnumerable<TaskToWorker> _registeredWorkers;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IPollTimingStrategy _pollTimingStrategy;
        private readonly IPollOrderStrategy _pollOrderStrategy;

        public ExecutionManager(
            WorkerSetConfig options,
            ILogger<ExecutionManager> logger,
            ITaskService taskService,
            IEnumerable<TaskToWorker> workerMappings,
            ILifetimeScope lifetimeScope,
            IPollTimingStrategy pollTimingStrategy,
            IPollOrderStrategy pollOrderStrategy
        )
        {
            _configuration = options;
            _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentWorkers);
            _logger = logger;
            _taskManager = taskService;
            _registeredWorkers = workerMappings;
            _lifetimeScope = lifetimeScope;
            _pollTimingStrategy = pollTimingStrategy;
            _pollOrderStrategy = pollOrderStrategy;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var currentSleepInterval = _configuration.SleepInterval;

            while (!cancellationToken.IsCancellationRequested)
            {
                var queuedTasks = (await _taskManager.GetAllQueues())
                    .Where(a => _registeredWorkers.Any(b => b.TaskName == a.Key) && a.Value > 0)
                    .ToDictionary(a => a.Key, a => a.Value);

                var scheduledWorkers = _registeredWorkers.Where(a => queuedTasks.ContainsKey(a.TaskName)).ToList();

                currentSleepInterval = _pollTimingStrategy.CalculateDelay(
                    queuedTasks,
                    scheduledWorkers,
                    _configuration.SleepInterval,
                    currentSleepInterval
                );

                scheduledWorkers = _pollOrderStrategy.CalculateOrder(queuedTasks, scheduledWorkers, _semaphore.CurrentCount);

                foreach (var scheduledWorker in scheduledWorkers)
                {
                    await _semaphore.WaitAsync(cancellationToken);
                    _ = PollAndHandle(scheduledWorker, cancellationToken).ContinueWith(_ => _semaphore.Release());
                }

                await Task.Delay(currentSleepInterval, cancellationToken);
            }
        }

        private Type GetInputType(Type workerType)
        {
            var interfaces = workerType
                .GetInterfaces()
                .Where(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(ITaskRequestHandler<,>))
                .First();
            var genericArguments = interfaces.GetGenericArguments();

            var inputType = genericArguments[0];
            var outputType = genericArguments[1];

            return inputType;
        }

        private async Task PollAndHandle(TaskToWorker scheduledWorker, CancellationToken cancellationToken)
        {
            var workerId = Guid.NewGuid().ToString();
            PollTaskResponse pollResponse;

            if (string.IsNullOrEmpty(_configuration.Domain))
                pollResponse = await _taskManager.PollTasks(scheduledWorker.TaskName, workerId);
            else
                pollResponse = await _taskManager.PollTasks(scheduledWorker.TaskName, workerId, _configuration.Domain);

            if (pollResponse == null)
                return;

            if (!string.IsNullOrEmpty(pollResponse.ExternalInputPayloadStorage))
            {
                _logger.LogDebug($"Fetching storage location {pollResponse.ExternalInputPayloadStorage}");
                var externalStorageLocation = await _taskManager.FetchExternalStorageLocation(pollResponse.ExternalInputPayloadStorage);
                pollResponse.InputData = await _taskManager.FetchExternalStorage(externalStorageLocation.Path);
            }

            try
            {
                var inputType = GetInputType(scheduledWorker.TaskType);
                var inputData = pollResponse.InputData.ToObject(inputType, ConductorConstants.IoJsonSerializer);

                using var scope = _lifetimeScope.BeginLifetimeScope();

                var context = scope.ResolveOptional<ConductorSharpExecutionContext>();
                var mediator = scope.Resolve<IMediator>();

                if (context != null)
                {
                    context.WorkflowName = pollResponse.WorkflowType;
                    context.TaskName = pollResponse.TaskDefName;
                    context.TaskReferenceName = pollResponse.ReferenceTaskName;
                    context.WorkflowId = pollResponse.WorkflowInstanceId;
                    context.CorrelationId = pollResponse.CorrelationId;
                    context.TaskId = pollResponse.TaskId;
                    context.WorkerId = workerId;
                }

                var response = await mediator.Send(inputData, cancellationToken);

                await _taskManager.UpdateTaskCompleted(response, pollResponse.TaskId, pollResponse.WorkflowInstanceId);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "{error} while executing {task} as part of {workflow} with id {workflowId}",
                    exception.Message,
                    pollResponse.TaskDefName,
                    pollResponse.WorkflowType,
                    pollResponse.WorkflowInstanceId
                );

                var errorMessage = new ErrorOutput { ErrorMessage = exception.Message };

                await _taskManager.UpdateTaskFailed(
                    errorMessage,
                    pollResponse.TaskId,
                    pollResponse.WorkflowInstanceId,
                    exception.Message,
                    exception.StackTrace
                );
            }
        }
    }
}
