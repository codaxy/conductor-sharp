using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class MetadataService(HttpClient client) : IMetadataService
    {
        private readonly ConductorClient _client = new(client);

        public async Task<ICollection<WorkflowDef>> ListWorkflowsAsync(CancellationToken cancellationToken = default) =>
            await _client.GetAllAsync(cancellationToken);

        public async Task<BulkResponse> UpdateWorkflowsAsync(IEnumerable<WorkflowDef> workflows, CancellationToken cancellationToken = default) =>
            await _client.UpdateAsync(workflows, cancellationToken);

        public async Task AddWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default) =>
            await _client.CreateAsync(workflowDef, cancellationToken);

        public async Task<ICollection<TaskDef>> ListTasksAsync(CancellationToken cancellationToken = default) =>
            await _client.GetTaskDefsAsync(cancellationToken);

        public async Task AddTaskAsync(TaskDef taskDef, CancellationToken cancellationToken = default) =>
            await _client.RegisterTaskDefAsync(taskDef, cancellationToken);

        public async Task AddTasksAsync(IEnumerable<TaskDef> taskDefs, CancellationToken cancellationToken = default) =>
            await _client.RegisterTaskDef_1Async(taskDefs, cancellationToken);

        public async Task ValidateWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default) =>
            await _client.ValidateAsync(workflowDef, cancellationToken);

        public async Task<WorkflowDef> GetWorkflowAsync(string name, int? version = null, CancellationToken cancellationToken = default) =>
            await _client.GetAsync(name, version, cancellationToken);

        public async Task<IDictionary<string, object>> GetWorkflowNamesAndVersionsAsync(CancellationToken cancellationToken = default) =>
            await _client.GetWorkflowNamesAndVersionsAsync(cancellationToken);

        public async Task<ICollection<WorkflowDef>> GetAllWorkflowsWithLatestVersionsAsync(CancellationToken cancellationToken = default) =>
            await _client.GetAllWorkflowsWithLatestVersionsAsync(cancellationToken);

        public async Task<TaskDef> GetTaskAsync(string taskType, CancellationToken cancellationToken = default) =>
            await _client.GetTaskDefAsync(taskType, cancellationToken);

        public async Task DeleteTaskAsync(string taskType, CancellationToken cancellationToken = default) =>
            await _client.UnregisterTaskDefAsync(taskType, cancellationToken);

        public async Task DeleteWorkflowAsync(string name, int version, CancellationToken cancellationToken = default) =>
            await _client.UnregisterWorkflowDefAsync(name, version, cancellationToken);
    }
}
