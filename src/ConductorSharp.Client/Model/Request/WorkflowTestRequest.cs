using ConductorSharp.Client.Model.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Client.Model.Request
{
    public class WorkflowTestRequest
    {
        public class TaskMock
        {
            public string Status { get; set; }
            public JObject Output { get; set; }
            public long ExecutionTime { get; set; }
            public long QueueWaitTime { get; set; }
            public JObject AdditionalProperties { get; set; }
        }

        public string Name { get; set; }
        public int Version { get; set; }
        public string CorrelationId { get; set; }
        public JObject Input { get; set; }
        public Dictionary<string, string> TaskToDomain { get; set; }
        public WorkflowDefinition WorkflowDef { get; set; }
        public string ExternalInputPayloadStoragePath { get; set; }
        public int Priority { get; set; }
        public Dictionary<string, List<TaskMock>> TaskRefToMockOutput { get; set; }
        public Dictionary<string, WorkflowTestRequest> SubWorkflowTestRequest { get; set; }
        public JObject AdditionalProperties { get; set; }
    }
}
