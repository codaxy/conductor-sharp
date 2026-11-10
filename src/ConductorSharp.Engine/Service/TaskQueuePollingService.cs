using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Health;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Service
{
    internal class TaskQueuePollingService
    {
        private const int DefaultMaxAttempts = 5;
        private static readonly TimeSpan DefaultInitialRetryDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DefaultMaxRetryDelay = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DefaultMaxJitter = TimeSpan.FromMilliseconds(500);

        private readonly ITaskService _taskService;
        private readonly ILogger<TaskQueuePollingService> _logger;
        private readonly IConductorSharpHealthService _healthService;
        private readonly int _maxAttempts;
        private readonly TimeSpan _initialRetryDelay;
        private readonly TimeSpan _maxRetryDelay;
        private readonly TimeSpan _maxJitter;
        private readonly Func<TimeSpan, CancellationToken, Task> _delay;
        private readonly Func<double> _jitter;

        public TaskQueuePollingService(ITaskService taskService, ILogger<TaskQueuePollingService> logger, IConductorSharpHealthService healthService)
            : this(
                taskService,
                logger,
                healthService,
                DefaultMaxAttempts,
                DefaultInitialRetryDelay,
                DefaultMaxRetryDelay,
                DefaultMaxJitter,
                Task.Delay,
                Random.Shared.NextDouble
            ) { }

        internal TaskQueuePollingService(
            ITaskService taskService,
            ILogger<TaskQueuePollingService> logger,
            IConductorSharpHealthService healthService,
            int maxAttempts,
            TimeSpan initialRetryDelay,
            TimeSpan maxRetryDelay,
            TimeSpan maxJitter,
            Func<TimeSpan, CancellationToken, Task> delay,
            Func<double> jitter
        )
        {
            _taskService = taskService;
            _logger = logger;
            _healthService = healthService;
            _maxAttempts = maxAttempts;
            _initialRetryDelay = initialRetryDelay;
            _maxRetryDelay = maxRetryDelay;
            _maxJitter = maxJitter;
            _delay = delay;
            _jitter = jitter;
        }

        public async Task<IDictionary<string, long>> ListQueuesAsync(CancellationToken cancellationToken)
        {
            var retryDelay = _initialRetryDelay;

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    var queues = await _taskService.ListQueuesAsync(cancellationToken);

                    // The engine counts as running once it reads the queues; the background service clears this on failure.
                    if (!(await _healthService.GetHealthData(cancellationToken)).IsExecutionManagerRunning)
                        await _healthService.SetExecutionManagerRunning(cancellationToken);

                    return queues;
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested && IsTransient(exception) && attempt < _maxAttempts)
                {
                    var delay = retryDelay + TimeSpan.FromMilliseconds(_jitter() * _maxJitter.TotalMilliseconds);

                    _logger.LogWarning(
                        exception,
                        "Failed to read Conductor task queues. Attempt {Attempt}/{MaxAttempts}; retrying in {RetryDelay}",
                        attempt,
                        _maxAttempts,
                        delay
                    );

                    await _delay(delay, cancellationToken);
                    retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 2, _maxRetryDelay.TotalMilliseconds));
                }
            }
        }

        private static bool IsTransient(Exception exception)
        {
            return exception is HttpRequestException
                || exception is TaskCanceledException
                || exception is ApiException apiException && (apiException.StatusCode is 408 or 429 || apiException.StatusCode >= 500);
        }
    }
}
