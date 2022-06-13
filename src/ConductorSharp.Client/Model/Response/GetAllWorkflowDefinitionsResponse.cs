using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Response
{

    public class GetAllWorkflowDefinitionsResponse
    {
        [JsonProperty("result")]
        public List<WorkflowDefinition> Result { get; set; }
    }
}