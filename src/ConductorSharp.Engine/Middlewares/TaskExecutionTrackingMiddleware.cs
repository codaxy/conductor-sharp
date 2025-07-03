using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Middlewares;

public class TaskExecutionTrackingMiddleware<TRequest, TResponse> : IWorkerMiddleware<TRequest, TResponse>
    where TRequest : ITaskInput<TResponse>, new()
{
    private readonly IEnumerable<ITaskExecutionService> _taskExecutionServices;

    public TaskExecutionTrackingMiddleware(IEnumerable<ITaskExecutionService> taskExecutionServices)
    {
        _taskExecutionServices = taskExecutionServices;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        WorkerExecutionContext context,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        var trackedTask = new RunningTask
        {
            TaskId = context.TaskId,
            TaskName = context.TaskName,
            StartedAt = DateTimeOffset.UtcNow
        };

        foreach (var taskExecutionService in _taskExecutionServices)
        {
            await taskExecutionService.OnPolled(trackedTask);
        }

        try
        {
            var response = await next();

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
