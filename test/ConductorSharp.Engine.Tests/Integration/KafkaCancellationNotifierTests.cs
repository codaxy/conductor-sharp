using ConductorSharp.KafkaCancellationNotifier;
using ConductorSharp.KafkaCancellationNotifier.Service;
using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using KafkaNotifier = ConductorSharp.KafkaCancellationNotifier.Service.KafkaCancellationNotifier;

namespace ConductorSharp.Engine.Tests.Integration;

public class KafkaCancellationNotifierTests : IAsyncLifetime
{
    private const string TaskName = "TEST_cancellable_task";
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(20);

    // librdkafka's in-process mock cluster speaks the real Kafka protocol, so no broker or Docker is needed.
    private readonly IProducer<Null, string> _producer = new ProducerBuilder<Null, string>(
        new Dictionary<string, string> { ["test.mock.num.brokers"] = "1" }
    ).Build();
    private readonly TopicPartition _partition = new("conductor.status.task", new Partition(0));
    private readonly KafkaNotifier _notifier = new(new[] { new TaskToWorker { TaskName = TaskName } }, NullLogger<KafkaNotifier>.Instance);
    private ICancellationNotifier.ICancellationTokenHolder _probe = null!;
    private KafkaConsumerBackgroundService _service = null!;

    public async Task InitializeAsync()
    {
        // Producing creates the topic, so the consumer subscribes to a topic that exists.
        await _producer.ProduceAsync(_partition, StatusEvent("warm-up-task", "COMPLETED"));

        using var admin = new DependentAdminClientBuilder(_producer.Handle).Build();
        var bootstrapServers = string.Join(",", admin.GetMetadata(WaitTimeout).Brokers.Select(b => $"{b.Host}:{b.Port}"));

        _service = new KafkaConsumerBackgroundService(
            Options.Create(
                new KafkaOptions
                {
                    BootstrapServers = bootstrapServers,
                    TopicName = _partition.Topic,
                    GroupId = "test"
                }
            ),
            NullLogger<KafkaConsumerBackgroundService>.Instance,
            _notifier
        );
        await _service.StartAsync(CancellationToken.None);

        // The consumer starts at the latest offset, so repeat a probe event until one is handled.
        _probe = _notifier.GetCancellationToken("probe-task", CancellationToken.None);
        var deadline = DateTime.UtcNow + WaitTimeout;
        do
        {
            Assert.True(DateTime.UtcNow < deadline, "The consumer did not start consuming");
            await _producer.ProduceAsync(_partition, StatusEvent("probe-task", "CANCELED"));
        } while (!_probe.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100)));
    }

    public async Task DisposeAsync()
    {
        using var stopTimeout = new CancellationTokenSource(WaitTimeout);
        await _service.StopAsync(stopTimeout.Token);
        _service.Dispose();
        _probe.Dispose();
        _producer.Dispose();
    }

    [Theory]
    [InlineData("""{"taskId":"other-task","workflowInstanceId":"workflow-1","status":"CANCELED","workflowTask":null}""")]
    [InlineData("not json")]
    public async Task CancelsTaskAfterUnprocessableMessage(string unprocessableMessage)
    {
        using var runningTask = _notifier.GetCancellationToken("running-task", CancellationToken.None);

        await _producer.ProduceAsync(_partition, new Message<Null, string> { Value = unprocessableMessage });
        await _producer.ProduceAsync(_partition, StatusEvent("running-task", "CANCELED"));

        Assert.True(runningTask.CancellationToken.WaitHandle.WaitOne(WaitTimeout), "The running task was not cancelled");
    }

    [Fact]
    public async Task StopAsyncEndsConsumption()
    {
        using var stopTimeout = new CancellationTokenSource(WaitTimeout);
        await _service.StopAsync(stopTimeout.Token);

        Assert.True(_service.ExecuteTask!.IsCompleted, "The consumer is still running after StopAsync");
    }

    private static Message<Null, string> StatusEvent(string taskId, string status) =>
        new()
        {
            Value = JsonConvert.SerializeObject(
                new
                {
                    taskId,
                    workflowInstanceId = "workflow-1",
                    status,
                    workflowTask = new
                    {
                        name = TaskName,
                        taskReferenceName = "task_ref",
                        type = "SIMPLE"
                    }
                }
            )
        };
}
