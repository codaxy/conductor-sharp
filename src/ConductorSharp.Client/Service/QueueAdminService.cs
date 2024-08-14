using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public class QueueAdminService(IHttpClientFactory httpClientFactory, string clientName) : IQueueAdminService
{
    private readonly ConductorClient _client = new(httpClientFactory.CreateClient(clientName));

    public async Task MarkWaitTaskCompletedAsync(
        string workflowId,
        string taskRefName,
        Status2 status,
        IDictionary<string, object> output,
        CancellationToken cancellationToken = default
    ) => await _client.Update_1Async(workflowId, taskRefName, status, output, cancellationToken);

    public async Task MarkWaitTaskCompletedAsync(
        string workflowId,
        string taskId,
        Generated.TaskStatus status,
        IDictionary<string, object> output,
        CancellationToken cancellationToken = default
    ) => await _client.UpdateByTaskIdAsync(workflowId, taskId, status, output, cancellationToken);

    public async Task<IDictionary<string, long>> GetQueueLengthAsync(CancellationToken cancellationToken = default) =>
        await _client.Size_1Async(cancellationToken);

    public async Task<IDictionary<string, string>> GetQueueNamesAsync(CancellationToken cancellationToken = default) =>
        await _client.NamesAsync(cancellationToken);
}
