using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json;

namespace ConductorSharp.Client.Model.Response
{

    public class GetWorkflowDefinitionResponse
    {
        [JsonProperty("result")]
        public WorkflowDefinition Result { get; set; }
    }
}