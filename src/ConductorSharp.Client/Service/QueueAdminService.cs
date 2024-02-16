using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service;

public class QueueAdminService(ConductorClient client) : IQueueAdminService
{
    public async Task MarkWaitTaskCompletedAsync(
        string workflowId,
        string taskRefName,
        Status status,
        IDictionary<string, object> output,
        CancellationToken cancellationToken = default
    ) => await client.Update_1Async(workflowId, taskRefName, status, output, cancellationToken);

    public async Task MarkWaitTaskCompletedAsync(
        string workflowId,
        string taskId,
        Status2 status,
        IDictionary<string, object> output,
        CancellationToken cancellationToken = default
    ) => await client.UpdateByTaskIdAsync(workflowId, taskId, status, output, cancellationToken);

    public async Task<IDictionary<string, long>> GetQueueLengthAsync(CancellationToken cancellationToken = default) =>
        await client.Size_1Async(cancellationToken);

    public async Task<IDictionary<string, string>> GetQueueNamesAsync(CancellationToken cancellationToken = default) =>
        await client.NamesAsync(cancellationToken);
}
