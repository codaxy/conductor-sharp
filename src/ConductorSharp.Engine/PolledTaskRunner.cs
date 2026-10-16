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
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine
{
    /// <summary>
    /// Runs a polled task through its handler and reports the outcome to the server. Every execution
    /// manager hands its polled tasks to this one.
    /// </summary>
    /// <remarks>
    /// Logs under the <see cref="ExecutionManager"/> category, where this code lived before it was
    /// shared, so existing log filters keep matching.
    /// </remarks>
    internal class PolledTaskRunner(
        ILogger<ExecutionManager> logger,
        ITaskService taskService,
        IExternalPayloadService externalPayloadService,
        IServiceScopeFactory lifetimeScope,
        ICancellationNotifier cancellationNotifier
    )
    {
        private readonly ILogger<ExecutionManager> _logger = logger;
        private readonly ITaskService _taskManager = taskService;
        private readonly IExternalPayloadService _externalPayloadService = externalPayloadService;
        private readonly IServiceScopeFactory _lifetimeScopeFactory = lifetimeScope;
        private readonly ICancellationNotifier _cancellationNotifier = cancellationNotifier;

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

        public async Task RunAsync(
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
                        "READ",
                        "TASK_INPUT",
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
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                if (context != null)
                {
                    context.WorkflowName = pollResponse.WorkflowType;
                    context.TaskName = pollResponse.TaskDefName;
                    context.TaskReferenceName = pollResponse.ReferenceTaskName;
                    context.WorkflowId = pollResponse.WorkflowInstanceId;
                    context.CorrelationId = pollResponse.CorrelationId;
                    context.Context = ConductorSharpExecutionContext.CopyContext(pollResponse.Context);
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

                // A worker may throw a StructuredErrorException to attach a sanitized, stable classification to the
                // failed task's output. Plain exceptions keep producing only error_message, preserving backward compatibility.
                var errorMessage = new ErrorOutput { ErrorMessage = exception.Message, StructuredError = StructuredError.FromException(exception) };

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
