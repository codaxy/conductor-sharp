using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Patterns.Tasks
{
    #region models
    public class ReadWorkflowTasksRequest : ITaskInput<ReadWorkflowTasksResponse>
    {
        /// <summary>
        /// Comma separated list of task reference names to be read from specified workflow
        /// </summary>
        public string? TaskNames { get; set; }

        /// <summary>
        /// Id of the workflow to read tasks from
        /// </summary>
        public string? WorkflowId { get; set; }
    }

    public record ReadWorkflowTasksResponse(Dictionary<string, Client.Generated.Task> Tasks, WorkflowDetails Workflow);

    public record WorkflowDetails(JObject InputData);

    #endregion
    /// <summary>
    /// Uses the Conductor API to read the input/output and status of the specified tasks for the specified workflow.
    /// </summary>
    [OriginalName(Constants.TaskNamePrefix + "_read_tasks")]
    public class ReadWorkflowTasks(IWorkflowService workflowService) : NgWorker<ReadWorkflowTasksRequest, ReadWorkflowTasksResponse>
    {
        private readonly IWorkflowService _workflowService = workflowService;

        public override async Task<ReadWorkflowTasksResponse> Handle(
            ReadWorkflowTasksRequest input,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(input.TaskNames))
            {
                throw new Exception("No task names provided. Comma separated list of reference names expected");
            }

            if (string.IsNullOrEmpty(input.WorkflowId))
            {
                throw new Exception("No workflowId provided");
            }

            var tasknames = input.TaskNames.Split(",").Where(a => !string.IsNullOrEmpty(a)).ToList();

            var starterWorkflow =
                await _workflowService.GetExecutionStatusAsync(input.WorkflowId, cancellationToken: cancellationToken)
                ?? throw new Exception($"Could not find starter workflow by id {input.WorkflowId}");

            var output = new ReadWorkflowTasksResponse([], new WorkflowDetails(JObject.FromObject(starterWorkflow.Input)));

            foreach (var task in tasknames)
            {
                output.Tasks.Add(task, starterWorkflow.Tasks.FirstOrDefault(a => a.ReferenceTaskName == task) ?? new Client.Generated.Task());
            }

            return output;
        }
    }
}
