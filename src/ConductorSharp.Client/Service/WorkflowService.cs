using System.Net.Http;
using ConductorSharp.Client.Generated;

namespace ConductorSharp.Client.Service
{
    public class WorkflowService(IHttpClientFactory httpClientFactory, string clientName) : IWorkflowService
    {
        private readonly ConductorClient _client = new(httpClientFactory.CreateClient(clientName));

        /// <summary>
        /// Skips a given task from a current running workflow
        /// </summary>
        public async Task SkipTaskAsync(
            string workflowId,
            string taskReferenceName,
            SkipTaskRequest skipTaskRequest,
            CancellationToken cancellationToken = default
        ) => await _client.SkipTaskFromWorkflowAsync(workflowId, taskReferenceName, skipTaskRequest, cancellationToken);

        /// <summary>
        /// Resumes the workflow
        /// </summary>
        public async Task ResumeAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.ResumeWorkflowAsync(workflowId, cancellationToken);

        /// <summary>
        /// Pauses the workflow
        /// </summary>
        public async Task PauseAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.PauseWorkflowAsync(workflowId, cancellationToken);

        /// <summary>
        /// Starts the decision task for a workflow
        /// </summary>
        public async Task DecideAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.DecideAsync(workflowId, cancellationToken);

        /// <summary>
        /// Start a new workflow with StartWorkflowRequest, which allows task to be executed in a domain
        /// </summary>
        /// <remarks>
        /// ConductorShap: There is another Conductor API endpoint with seemingly identical functionality and operation
        /// id 'startWorkflow_1'. It seems these two achieve the same task so the second one has been left out.
        /// </remarks>
        public async Task<string> StartAsync(StartWorkflowRequest startWorkflowRequest, CancellationToken cancellationToken = default) =>
            await _client.StartWorkflowAsync(startWorkflowRequest, cancellationToken);

        /// <summary>
        /// Retries the last failed task
        /// </summary>
        public async Task RetryAsync(string workflowId, bool? resumeSubworkflowTasks = null, CancellationToken cancellationToken = default) =>
            await _client.RetryAsync(workflowId, resumeSubworkflowTasks, cancellationToken);

        /// <summary>
        /// Restarts a completed workflow
        /// </summary>
        public async Task RestartAsync(string workflowId, bool? useLatestDefinitions = null, CancellationToken cancellationToken = default) =>
            await _client.RestartAsync(workflowId, useLatestDefinitions, cancellationToken);

        /// <summary>
        /// Resets callback times of all non-terminal SIMPLE tasks to 0
        /// </summary>
        public async Task ResetCallbacksAsync(string workflowId, CancellationToken cancellationToken = default) =>
            await _client.ResetWorkflowAsync(workflowId, cancellationToken);

        /// <summary>
        /// Reruns the workflow from a specific task
        /// </summary>
        public async Task<string> RerunAsync(
            string workflowId,
            RerunWorkflowRequest rerunWorkflowRequest,
            CancellationToken cancellationToken = default
        ) => await _client.RerunAsync(workflowId, rerunWorkflowRequest, cancellationToken);

        /// <summary>
        /// Lists workflows for the given correlation id list
        /// </summary>
        public async Task<IDictionary<string, ICollection<Workflow>>> GetCorrelatedAsync(
            string name,
            IEnumerable<string> correlationIds,
            bool? includeClosed = false,
            bool? includeTasks = false,
            CancellationToken cancellationToken = default
        ) => await _client.GetWorkflowsAsync(name, includeClosed, includeTasks, correlationIds, cancellationToken);

        /// <summary>
        /// Test workflow execution using mock data
        /// </summary>
        public async Task<Workflow> TestAsync(WorkflowTestRequest workflowTestRequest, CancellationToken cancellationToken = default) =>
            await _client.TestWorkflowAsync(workflowTestRequest, cancellationToken);

        /// <summary>
        /// Gets the workflow by workflow id
        /// </summary>
        public async Task<Workflow> GetExecutionStatusAsync(
            string workflowId,
            bool? includeTasks = false,
            CancellationToken cancellationToken = default
        ) => await _client.GetExecutionStatusAsync(workflowId, includeTasks, cancellationToken);

        /// <summary>
        /// Terminate workflow execution
        /// </summary>
        public async Task TerminateAsync(string workflowId, string? reason = null, CancellationToken cancellationToken = default) =>
            await _client.Terminate_1Async(workflowId, reason, cancellationToken);

        /// <summary>
        /// Lists workflows for the given correlation id
        /// </summary>
        public async Task<ICollection<Workflow>> ListCorrelatedAsync(
            string name,
            string correlationId,
            bool? includeClosed = false,
            bool? includeTasks = false,
            CancellationToken cancellationToken = default
        ) => await _client.GetWorkflows_1Async(name, correlationId, includeClosed, includeTasks, cancellationToken);

        /// <summary>
        /// Search for workflows based on payload and other parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SearchResultWorkflowSummary> SearchAsync(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        ) => await _client.SearchAsync(start, size, sort, freeText, query, cancellationToken);

        /// <summary>
        /// Search for workflows based on payload and other parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SearchResultWorkflow> SearchV2Async(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        ) => await _client.SearchV2Async(start, size, sort, freeText, query, cancellationToken);

        /// <summary>
        /// Search for workflows based on task parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SearchResultWorkflowSummary> SearchByTasksAsync(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        ) => await _client.SearchWorkflowsByTasksAsync(start, size, sort, freeText, query, cancellationToken);

        /// <summary>
        /// Search for workflows based on task parameters. Use sort options as sort=<field>:ASC|DESC e.g. sort=name&sort=workflowId:DESC. If order is not specified, defaults to ASC.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="sort"></param>
        /// <param name="freeText"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SearchResultWorkflow> SearchV2ByTasksAsync(
            int? start = null,
            int? size = null,
            string? sort = null,
            string? freeText = null,
            string? query = null,
            CancellationToken cancellationToken = default
        ) => await _client.SearchWorkflowsByTasksV2Async(start, size, sort, freeText, query, cancellationToken);

        /// <summary>
        /// Retrieve all the running workflows
        /// </summary>
        public async Task<ICollection<string>> ListRunningAsync(
            string name,
            int? version,
            long? startTime = null,
            long? endTime = null,
            CancellationToken cancellationToken = default
        ) => await _client.GetRunningWorkflowAsync(name, version, startTime, endTime, cancellationToken);

        /// <summary>
        /// Get the uri and path of the external storage where the workflow payload is to be stored
        /// </summary>
        public async Task GetExternalStorageLocationAsync(
            string path,
            string operation,
            string payloadType,
            CancellationToken cancellationToken = default
        ) => await _client.GetExternalStorageLocationAsync(path, operation, payloadType, cancellationToken);

        /// <summary>
        /// Removes the workflow from the system
        /// </summary>
        public async Task DeleteAsync(string workflowId, bool? archiveWorkflow = null, CancellationToken cancellationToken = default) =>
            await _client.DeleteAsync(workflowId, archiveWorkflow, cancellationToken);
    }
}
