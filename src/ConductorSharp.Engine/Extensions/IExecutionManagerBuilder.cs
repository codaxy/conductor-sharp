using System;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Polling;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Extensions
{
    public interface IExecutionManagerBuilder
    {
        IServiceCollection Builder { get; set; }
        IExecutionManagerBuilder AddPipelines(Action<IPipelineBuilder> pipelines);
        IExecutionManagerBuilder SetHealthCheckService<T>()
            where T : IConductorSharpHealthService;
        IExecutionManagerBuilder UseConstantPollTimingStrategy();
        IExecutionManagerBuilder UseBetaExecutionManager();

        /// <summary>
        /// Polls a task type when the transport announces that tasks of it may be waiting, and keeps
        /// polling it until its queue is empty, instead of reading the queue sizes on every sleep
        /// interval. An idle worker then sends nothing but the fallback poll.
        /// </summary>
        /// <remarks>
        /// Needs a transport that registers <see cref="Client.Service.ITaskAvailabilityNotifications"/>,
        /// such as Maestro's NATS transport. Conductor's REST API has no such notices.
        /// </remarks>
        /// <param name="fallbackPollInterval">
        /// How often every task type is polled anyway, in milliseconds. It picks up tasks whose
        /// notice was lost, so it bounds how long such a task waits.
        /// </param>
        IExecutionManagerBuilder UseTaskAvailabilityNotifications(int fallbackPollInterval = 30000);
    }
}
