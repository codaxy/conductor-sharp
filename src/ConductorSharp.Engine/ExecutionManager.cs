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
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine
{
    internal class ExecutionManager : IExecutionManager
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
        private readonly ICancellationNotifier _cancellationNotifier;
        private readonly WorkerInvokerService _workerInvokerService;

        public ExecutionManager(
            WorkerSetConfig options,
            ILogger<ExecutionManager> logger,
            ITaskService taskService,
            IEnumerable<TaskToWorker> workerMappings,
            IExternalPayloadService externalPayloadService,
            IServiceScopeFactory lifetimeScope,
            IPollTimingStrategy pollTimingStrategy,
            IPollOrderStrategy pollOrderStrategy,
            ICancellationNotifier cancellationNotifier,
            WorkerInvokerService workerInvokerService
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
            _cancellationNotifier = cancellationNotifier;
            _workerInvokerService = workerInvokerService;
            _externalPayloadService = externalPayloadService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var currentSleepInterval = _configuration.SleepInterval;

            while (!cancellationToken.IsCancellationRequested)
            {
                var queuedTasks = (await _taskManager.ListQueuesAsync(cancellationToken))
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
                    await _semaphore.WaitAsync(cancellationToken);
                    _ = PollAndHandle(scheduledWorker, cancellationToken).ContinueWith(_ => _semaphore.Release());
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

            await ProcessPolledTask(pollResponse, workerId, scheduledWorker, cancellationToken);
        }

        private async Task ProcessPolledTask(
            Client.Generated.Task pollResponse,
            string workerId,
            TaskToWorker scheduledWorker,
            CancellationToken cancellationToken
        )
        {
            using var tokenHolder = _cancellationNotifier.GetCancellationToken(pollResponse.TaskId, cancellationToken);

            try
            {
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
                    var file = await _externalPayloadService.GetExternalStorageDataAsync(externalStorageLocation.Path, tokenHolder.CancellationToken);

                    using TextReader textReader = new StreamReader(file.Stream);
                    var json = await textReader.ReadToEndAsync();

                    pollResponse.InputData = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                        json,
                        ConductorConstants.IoJsonSerializerSettings
                    );
                }

                var inputType = GetInputType(scheduledWorker.TaskType);
                var inputData = SerializationHelper.DictonaryToObject(inputType, pollResponse.InputData, ConductorConstants.IoJsonSerializerSettings);
                // Poll response data can be huge (if read from external storage)
                // We can save memory by not holding reference to pollResponse.InputData after it is parsed
                pollResponse.InputData = null;

                using var scope = _lifetimeScopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetService<ConductorSharpExecutionContext>();

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

                var response = await mediator.Send(inputData, tokenHolder.CancellationToken);

                await _taskManager.UpdateAsync(
                    new TaskResult
                    {
                        TaskId = pollResponse.TaskId,
                        Status = TaskResultStatus.COMPLETED,
                        OutputData = SerializationHelper.ObjectToDictionary(response, ConductorConstants.IoJsonSerializerSettings),
                        WorkflowInstanceId = pollResponse.WorkflowInstanceId
                    },
                    tokenHolder.CancellationToken
                );
            }
            catch (OperationCanceledException) when (tokenHolder.IsCancellationRequestedByNotifier)
            {
                _logger.LogWarning(
                    "Polled task {Task}(id={TaskId}) of workflow {Workflow}(id={WorkflowId}) is cancelled",
                    pollResponse.TaskDefName,
                    pollResponse.TaskId,
                    pollResponse.WorkflowType,
                    pollResponse.WorkflowInstanceId
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) // This is fine since we know cancellationToken comes from background service
            {
                _logger.LogWarning(
                    "Cancelling task {Task}(id={TaskId}) of workflow {Workflow}(id={WorkflowId}) due to background service shutdown",
                    pollResponse.TaskDefName,
                    pollResponse.TaskId,
                    pollResponse.WorkflowType,
                    pollResponse.WorkflowInstanceId
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
                                WorkflowInstanceId = pollResponse?.WorkflowInstanceId
                            },
                            tokenHolder.CancellationToken
                        ),
                        _taskManager.LogAsync(pollResponse.TaskId, exception.Message, tokenHolder.CancellationToken),
                        _taskManager.LogAsync(pollResponse.TaskId, exception.StackTrace, tokenHolder.CancellationToken)
                    ]
                );
            }
        }
    }
}
