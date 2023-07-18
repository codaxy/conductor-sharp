using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class WorkflowSearchResponse
    {
        public class Data
        {
            public string WorkflowType { get; set; }
            public int Version { get; set; }
            public string WorkflowId { get; set; }
            public string CorrelationId { get; set; }
            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset UpdateTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
            public string Status { get; set; }
            public string Input { get; set; }
            public string Output { get; set; }
            public string ReasonForIncompletion { get; set; }
            public long ExecutionTime { get; set; }
            public string Event { get; set; }
            public string FailedReferenceTaskNames { get; set; }
            public string ExternalInputPayloadStoragePath { get; set; }
            public string ExternalOutputPayloadStoragePath { get; set; }
            public int Priority { get; set; }
            public List<string> FailedTaskNames { get; set; } = new List<string>();
            public long OutputSize { get; set; }
            public long InputSize { get; set; }
        }

        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }

        [JsonProperty("results")]
        public List<Data> Results { get; set; }
    }
}
