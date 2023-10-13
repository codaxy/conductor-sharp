using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Behaviors
{
    public class TaskExecutionTrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ConductorSharpExecutionContext _executionContext;
        private readonly IEnumerable<ITaskExecutionService> _taskExecutionServices;

        public TaskExecutionTrackingBehavior(
            ConductorSharpExecutionContext executionContext,
            IEnumerable<ITaskExecutionService> taskExecutionServices
        )
        {
            _executionContext = executionContext;
            _taskExecutionServices = taskExecutionServices;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
}
