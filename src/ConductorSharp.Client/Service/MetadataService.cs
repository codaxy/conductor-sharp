using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class MetadataService(ConductorClient client) : IMetadataService
    {
        public async Task<ICollection<WorkflowDef>> ListWorkflowsAsync(CancellationToken cancellationToken = default) =>
            await client.GetAllAsync(cancellationToken);

        public async Task<BulkResponse> UpdateWorkflowsAsync(IEnumerable<WorkflowDef> workflows, CancellationToken cancellationToken = default) =>
            await client.UpdateAsync(workflows, cancellationToken);

        public async Task AddWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default) =>
            await client.CreateAsync(workflowDef, cancellationToken);

        public async Task<ICollection<TaskDef>> ListTasksAsync(CancellationToken cancellationToken = default) =>
            await client.GetTaskDefsAsync(cancellationToken);

        public async Task AddTaskAsync(TaskDef taskDef, CancellationToken cancellationToken = default) =>
            await client.RegisterTaskDefAsync(taskDef, cancellationToken);

        public async Task AddTasksAsync(IEnumerable<TaskDef> taskDefs, CancellationToken cancellationToken = default) =>
            await client.RegisterTaskDef_1Async(taskDefs, cancellationToken);

        public async Task ValidateWorkflowAsync(WorkflowDef workflowDef, CancellationToken cancellationToken = default) =>
            await client.ValidateAsync(workflowDef, cancellationToken);

        public async Task<WorkflowDef> GetWorkflowAsync(string name, int? version = null, CancellationToken cancellationToken = default) =>
            await client.GetAsync(name, version, cancellationToken);

        public async Task<IDictionary<string, object>> GetWorkflowNamesAndVersionsAsync(CancellationToken cancellationToken = default) =>
            await client.GetWorkflowNamesAndVersionsAsync(cancellationToken);

        public async Task<ICollection<WorkflowDef>> GetAllWorkflowsWithLatestVersionsAsync(CancellationToken cancellationToken = default) =>
            await client.GetAllWorkflowsWithLatestVersionsAsync(cancellationToken);

        public async Task<TaskDef> GetTaskAsync(string taskType, CancellationToken cancellationToken = default) =>
            await client.GetTaskDefAsync(taskType, cancellationToken);

        public async Task DeleteTaskAsync(string taskType, CancellationToken cancellationToken = default) =>
            await client.UnregisterTaskDefAsync(taskType, cancellationToken);

        public async Task DeleteWorkflowAsync(string name, int version, CancellationToken cancellationToken = default) =>
            await client.UnregisterWorkflowDefAsync(name, version, cancellationToken);
    }
}
