using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class WorkflowSearchResponse
    {
        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }

        [JsonProperty("results")]
        public List<WorkflowStatusResponse> Results { get; set; }
    }
}
