using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ConductorSharp.Client;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Client.Util;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Polling;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine
{
    internal class ExecutionManager
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly WorkerSetConfig _configuration;
        private readonly ILogger<ExecutionManager> _logger;
        private readonly ITaskService _taskManager;
        private readonly IExternalPayloadService _externalPayloadService;
        private readonly IEnumerable<TaskToWorker> _registeredWorkers;
        private readonly IServiceScopeFactory _lifetimeScopeFactory;
        private readonly IPollTimingStrategy _pollTimingStrategy;
        private readonly IPollOrderStrategy _pollOrderStrategy;

        public ExecutionManager(
            WorkerSetConfig options,
            ILogger<ExecutionManager> logger,
            ITaskService taskService,
            IEnumerable<TaskToWorker> workerMappings,
            IExternalPayloadService externalPayloadService,
            IServiceScopeFactory lifetimeScope,
            IPollTimingStrategy pollTimingStrategy,
            IPollOrderStrategy pollOrderStrategy
        )
        {
            _configuration = options;
            _semaphore = new SemaphoreSlim(_configuration.MaxConcurrentWorkers);
            _logger = logger;
            _taskManager = taskService;
            _registeredWorkers = workerMappings;
            _lifetimeScopeFactory = lifetimeScope;
            _pollTimingStrategy = pollTimingStrategy;
            _pollOrderStrategy = pollOrderStrategy;
            _externalPayloadService = externalPayloadService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var currentSleepInterval = _configuration.SleepInterval;

            while (!cancellationToken.IsCancellationRequested)
            {
                var queuedTasks = (await _taskManager.ListQueuesAsync(cancellationToken))
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

        private static Type GetInputType(Type workerType)
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

            var pollResponse = await _taskManager.PollAsync(scheduledWorker.TaskName, workerId, _configuration.Domain, cancellationToken);

            if (pollResponse == null)
                return;

            if (!string.IsNullOrEmpty(pollResponse.ExternalInputPayloadStoragePath))
            {
                _logger.LogDebug("Fetching storage {location}", pollResponse.ExternalInputPayloadStoragePath);
                // TODO: Check what the operation and payload type are
                var externalStorageLocation = await _taskManager.GetExternalStorageLocationAsync(
                    pollResponse.ExternalInputPayloadStoragePath,
                    "",
                    "",
                    cancellationToken
                );

                // TODO: iffy
                var file = await _externalPayloadService.GetExternalStorageDataAsync(externalStorageLocation.Path, cancellationToken);

                using TextReader textReader = new StreamReader(file.Stream);
                var json = await textReader.ReadToEndAsync();

                pollResponse.InputData = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                    json,
                    ConductorConstants.IoJsonSerializerSettings
                );
            }

            try
            {
                var inputType = GetInputType(scheduledWorker.TaskType);
                var inputData = SerializationHelper.DictonaryToObject(inputType, pollResponse.InputData, ConductorConstants.IoJsonSerializerSettings);
                // Poll response data can be huge (if read from external storage)
                // We can save memory by not holding reference to pollResponse.InputData after it is parsed
                pollResponse.InputData = null;

                using var scope = _lifetimeScopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetService<ConductorSharpExecutionContext>();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

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

                await _taskManager.UpdateAsync(
                    new TaskResult
                    {
                        TaskId = pollResponse.TaskId,
                        Status = TaskResultStatus.COMPLETED,
                        OutputData = SerializationHelper.ObjectToDictionary(response, ConductorConstants.IoJsonSerializerSettings),
                        WorkflowInstanceId = pollResponse.WorkflowInstanceId
                    },
                    cancellationToken
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    "{@Exception} while executing {Task} as part of {Workflow} with id {WorkflowId}",
                    exception,
                    pollResponse.TaskDefName,
                    pollResponse.WorkflowType,
                    pollResponse.WorkflowInstanceId
                );

                var errorMessage = new ErrorOutput { ErrorMessage = exception.Message };

                // TODO: We should verify that this is alright, it is possible that when executed concurrently,
                // the updates caused by LogAsync will be discarded because the call of UpdateAsync(TaskResult...)
                // sets the logs to null. Not sure how this is implemented in the backend, also, would have expected this to be a
                // PUT or PATCH request, but by specs it is POST
                await Task.WhenAll(
                    [
                        _taskManager.UpdateAsync(
                            new TaskResult
                            {
                                TaskId = pollResponse.TaskId,
                                Status = TaskResultStatus.FAILED,
                                ReasonForIncompletion = exception.Message,
                                OutputData = SerializationHelper.ObjectToDictionary(errorMessage, ConductorConstants.IoJsonSerializerSettings),
                                WorkflowInstanceId = pollResponse.WorkflowInstanceId
                            },
                            cancellationToken
                        ),
                        _taskManager.LogAsync(pollResponse.TaskId, exception.Message, cancellationToken),
                        _taskManager.LogAsync(pollResponse.TaskId, exception.StackTrace, cancellationToken)
                    ]
                );
            }
        }
    }
}
