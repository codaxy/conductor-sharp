using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class WorkflowStatusResponse
    {
        [JsonProperty("workflowType")]
        public string WorkflowType { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("workflowId")]
        public string WorkflowId { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("input")]
        public string Input { get; set; }

        [JsonProperty("output")]
        public string Output { get; set; }

        [JsonProperty("executionTime")]
        public int ExecutionTime { get; set; }

        [JsonProperty("failedReferenceTaskNames")]
        public string FailedReferenceTaskNames { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("inputSize")]
        public int InputSize { get; set; }

        [JsonProperty("outputSize")]
        public int OutputSize { get; set; }
    }
}
