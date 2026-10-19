using System.Net;
using System.Text;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Health;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Unit;

[Collection(nameof(InMemoryHealthService))] // InMemoryHealthService keeps its flag in a static field
public class TaskQueuePollingServiceTests
{
    private readonly InMemoryHealthService _healthService = new();

    public TaskQueuePollingServiceTests() => _healthService.ResetHealthData().Wait();

    [Fact]
    public async Task ListQueuesAsync_MarksEngineRunning_AfterSuccessfulRead()
    {
        var service = CreateService(new SequenceHandler(HttpStatusCode.InternalServerError, HttpStatusCode.OK), []);

        await service.ListQueuesAsync(CancellationToken.None);

        Assert.True((await _healthService.GetHealthData()).IsExecutionManagerRunning);
    }

    [Fact]
    public async Task ListQueuesAsync_DoesNotMarkEngineRunning_WhenReadsFail()
    {
        var service = CreateService(new SequenceHandler(HttpStatusCode.Unauthorized), []);

        await Assert.ThrowsAsync<ApiException>(() => service.ListQueuesAsync(CancellationToken.None));

        Assert.False((await _healthService.GetHealthData()).IsExecutionManagerRunning);
    }

    [Fact]
    public async Task ListQueuesAsync_RetriesTransientFailuresAndReturnsQueues()
    {
        var handler = new SequenceHandler(HttpStatusCode.InternalServerError, HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);
        var delays = new List<TimeSpan>();
        var service = CreateService(handler, delays);

        var queues = await service.ListQueuesAsync(CancellationToken.None);

        Assert.Equal(3, handler.RequestCount);
        Assert.Equal(2, delays.Count);
        Assert.Equal(2, queues["test-task"]);
    }

    [Fact]
    public async Task ListQueuesAsync_DoesNotRetryNonTransientApiErrors()
    {
        var handler = new SequenceHandler(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        var delays = new List<TimeSpan>();
        var service = CreateService(handler, delays);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.ListQueuesAsync(CancellationToken.None));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(1, handler.RequestCount);
        Assert.Empty(delays);
    }

    [Fact]
    public async Task ListQueuesAsync_RethrowsAfterRetryLimit()
    {
        var handler = new SequenceHandler(
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError
        );
        var delays = new List<TimeSpan>();
        var service = CreateService(handler, delays);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.ListQueuesAsync(CancellationToken.None));

        Assert.Equal(500, exception.StatusCode);
        Assert.Equal(5, handler.RequestCount);
        Assert.Equal(4, delays.Count);
    }

    private TaskQueuePollingService CreateService(HttpMessageHandler handler, ICollection<TimeSpan> delays)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://conductor/") };
        var taskService = new TaskService(httpClient);

        return new TaskQueuePollingService(
            taskService,
            NullLogger<TaskQueuePollingService>.Instance,
            _healthService,
            maxAttempts: 5,
            initialRetryDelay: TimeSpan.FromSeconds(1),
            maxRetryDelay: TimeSpan.FromSeconds(30),
            maxJitter: TimeSpan.FromMilliseconds(500),
            delay: (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            },
            jitter: () => 0
        );
    }

    private sealed class SequenceHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses = new(statuses);

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var status = _statuses.Dequeue();
            var content = status == HttpStatusCode.OK ? """{"test-task":2}""" : "{}";

            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(content, Encoding.UTF8, "application/json"), });
        }
    }
}
