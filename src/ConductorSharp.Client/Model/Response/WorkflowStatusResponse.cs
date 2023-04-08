using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class WorkflowStatusResponse
    {
        public string OwnerApp { get; set; }
        public long CreateTime { get; set; }
        public long UpdateTime { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string Status { get; set; }
        public string WorkflowName { get; set; }
        public List<string> FailedReferenceTaskNames { get; set; } = new List<string>();
        public int WorkflowVersion { get; set; }
        public string WorkflowId { get; set; }
        public string CorrelationId { get; set; }
        public string ReasonForIncompletion { get; set; }
        public JObject Input { get; set; }
        public JObject Output { get; set; }
        public int Priority { get; set; }
        public long LastRetriedTime { get; set; }
        public WorkflowDefinition WorkflowDefinition { get; set; }
        public List<TaskStatusResponse> Tasks { get; set; }
    }
}
