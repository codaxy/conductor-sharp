using ConductorSharp.Client.Service;
using ConductorSharp.Engine;
using ConductorSharp.Engine.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Patterns.Tasks
{
    #region models
    public class ReadWorkflowTasksRequest : IRequest<ReadWorkflowTasksResponse>
    {
        /// <summary>
        /// Comma separated list of task reference names to be read from specified workflow
        /// </summary>
        [Required]
        public string TaskNames { get; set; }

        /// <summary>
        /// Id of the workflow to read tasks from
        /// </summary>
        [Required]
        public string WorkflowId { get; set; }
    }

    public class ReadWorkflowTasksResponse
    {
        public Dictionary<string, TaskExecutionDetails> Tasks { get; set; }
        public WorkflowDetails Workflow { get; set; }
    }

    public class WorkflowDetails
    {
        public JObject InputData { get; set; }
    }

    public class TaskExecutionDetails
    {
        [JsonProperty("inputData")]
        public JObject InputData { get; set; } = new JObject();

        [JsonProperty("outputData")]
        public JObject OutputData { get; set; } = new JObject();

        [JsonProperty("referenceTaskName")]
        public string ReferenceTaskName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "NOT_FOUND";
    }
    #endregion
    /// <summary>
    /// Uses the Conductor API to read the input/output and status of the specified tasks for the specified workflow.
    /// </summary>
    [OriginalName(Constants.TaskNamePrefix + "_read_tasks")]
    public class ReadWorkflowTasks : TaskRequestHandler<ReadWorkflowTasksRequest, ReadWorkflowTasksResponse>
    {
        private readonly ILogger<ReadWorkflowTasks> _logger;
        private readonly IWorkflowService _workflowService;

        public ReadWorkflowTasks(ILogger<ReadWorkflowTasks> logger, IWorkflowService workflowService)
        {
            _logger = logger;
            _workflowService = workflowService;
        }

        public async override Task<ReadWorkflowTasksResponse> Handle(ReadWorkflowTasksRequest input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input.TaskNames))
                throw new Exception("No task names provided. Comma separated list of reference names expected");

            var tasknames = input.TaskNames.Split(",").Where(a => !string.IsNullOrEmpty(a)).ToList();

            var starterWorkflow = await _workflowService.GetWorkflowStatus(input.WorkflowId);

            if (starterWorkflow == null)
                throw new Exception($"Could not find starter workflow by id {input.WorkflowId}");

            var taskData = starterWorkflow.SelectToken("tasks").ToObject<List<TaskExecutionDetails>>();

            var output = new ReadWorkflowTasksResponse
            {
                Workflow = new WorkflowDetails { InputData = starterWorkflow.SelectToken("input") as JObject },
                Tasks = new Dictionary<string, TaskExecutionDetails>()
            };

            foreach (var task in tasknames)
            {
                output.Tasks.Add(task, taskData.FirstOrDefault(a => a.ReferenceTaskName == task) ?? new TaskExecutionDetails());
            }

            return output;
        }
    }
}
