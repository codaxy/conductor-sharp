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

namespace ConductorSharp.Engine
{
    public class ExecutionManager
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly WorkerSetConfig _configuration;
        private readonly ILogger<ExecutionManager> _logger;
        private readonly ITaskService _taskManager;
        private readonly IEnumerable<TaskToWorker> _registeredWorkers;
        private readonly ILifetimeScope _lifetimeScope;

        // TODO: Implement polling strategy so that if there
        // are no requests incoming we poll less, and when queues are full
        // we poll more often

        // TODO: Implement load balancing strategy so we can avoid
        // task starvation. One way is to create some sort of task priority
        // which will dynamically change based on how many of those tasks we served
        // When a task is polled we reduce its priorty and when increse the priorty of all others

        public ExecutionManager(
            WorkerSetConfig options,
            ILogger<ExecutionManager> logger,
            ITaskService taskService,
            IEnumerable<TaskToWorker> workerMappings,
            ILifetimeScope lifetimeScope
        )
        {
            _configuration = options;
            _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentWorkers);
            _logger = logger;
            _taskManager = taskService;
            _registeredWorkers = workerMappings;
            _lifetimeScope = lifetimeScope;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var scheduleQueue = await _taskManager.GetAllQueues();
                var scheduledWorkers = _registeredWorkers.Where(a => scheduleQueue.ContainsKey(a.TaskName) && scheduleQueue[a.TaskName] > 0).ToList();

                foreach (var scheduledWorker in scheduledWorkers)
                {
                    await _semaphore.WaitAsync(cancellationToken);
                    _ = PollAndHandle(scheduledWorker, cancellationToken).ContinueWith(_ => _semaphore.Release());
                }

                await Task.Delay(_configuration.SleepInterval, cancellationToken);
            }
        }

        private Type GetInputType(Type workerType)
        {
            var interfaces = workerType.GetInterfaces().Where(a => a.GetGenericTypeDefinition() == typeof(ITaskRequestHandler<,>)).First();
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
                await _taskManager.UpdateTaskFailed(pollResponse.TaskId, pollResponse.WorkflowInstanceId, exception.Message, exception.StackTrace);
            }
        }
    }
}
