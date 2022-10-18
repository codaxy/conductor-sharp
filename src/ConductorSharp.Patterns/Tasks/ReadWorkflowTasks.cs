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
    public class ReadWorkflowTasksRequest : IRequest<ReadWorkflowTasksResponse>
    {
        [Required]
        public string TaskNames { get; set; }

        [Required]
        public string WorkflowId { get; set; }
    }

    public class ReadWorkflowTasksResponse
    {
        public JObject Tasks { get; set; }
    }

    public class TaskExecutionDetails
    {
        public JObject InputData { get; set; }
        public JObject OutputData { get; set; }
        public string ReferenceTaskName { get; set; }
        public string Status { get; set; }
    }

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

            var output = new ReadWorkflowTasksResponse { Tasks = new JObject() };

            var taskNotFoundPrototype = new TaskExecutionDetails
            {
                InputData = new JObject(),
                OutputData = new JObject(),
                Status = "NOT_FOUND"
            };

            var starterWorkfow = await _workflowService.GetWorkflowStatus(input.WorkflowId);

            if (starterWorkfow == null)
                throw new Exception($"Could not find starter workflow by id {input.WorkflowId}");

            var taskData = starterWorkfow.SelectToken("tasks").ToObject<List<TaskExecutionDetails>>();

            foreach (var task in tasknames)
            {
                var value =
                    taskData.Where(a => a.ReferenceTaskName == task).Select(a => JObject.FromObject(a)).FirstOrDefault()
                    ?? JObject.FromObject(taskNotFoundPrototype);
                output.Tasks.Add(new JProperty(task, value));
            }

            // Add workflow input as it might also be useful
            output.Tasks.Add(new JProperty("workflow", starterWorkfow.SelectToken("input") as JObject));

            return output;
        }
    }
}
