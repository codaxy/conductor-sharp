using System.Net;
using System.Text;
using ConductorSharp.Client.Generated;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace ConductorSharp.Engine.Tests.Unit;

public class TaskQueuePollingServiceTests
{
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

    private static TaskQueuePollingService CreateService(HttpMessageHandler handler, ICollection<TimeSpan> delays)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://conductor/") };
        var taskService = new TaskService(httpClient);

        return new TaskQueuePollingService(
            taskService,
            NullLogger<TaskQueuePollingService>.Instance,
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
