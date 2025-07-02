using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Middlewares;

public class TaskExecutionTrackingMiddleware<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
    where TRequest : class, ITaskInput<TResponse>, new()
{
    private readonly WorkerExecutionContext _executionContext;
    private readonly IEnumerable<ITaskExecutionService> _taskExecutionServices;

    public TaskExecutionTrackingMiddleware(WorkerExecutionContext executionContext, IEnumerable<ITaskExecutionService> taskExecutionServices)
    {
        _executionContext = executionContext;
        _taskExecutionServices = taskExecutionServices;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        WorkerExecutionContext context,
        Func<TRequest, CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        var trackedTask = new RunningTask
        {
            TaskId = _executionContext.TaskId,
            TaskName = _executionContext.TaskName,
            StartedAt = DateTimeOffset.UtcNow
        };

        foreach (var taskExecutionService in _taskExecutionServices)
        {
            await taskExecutionService.OnPolled(trackedTask);
        }

        try
        {
            var response = await next(request, cancellationToken);

            foreach (var taskExecutionService in _taskExecutionServices)
            {
                await taskExecutionService.OnCompleted(trackedTask);
            }

            return response;
        }
        catch (Exception)
        {
            foreach (var taskExecutionService in _taskExecutionServices)
            {
                await taskExecutionService.OnFailed(trackedTask);
            }

            throw;
        }
    }
}
