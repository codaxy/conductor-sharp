using Newtonsoft.Json;
using System.Collections.Generic;
using XgsPon.Workflows.Client.Model.Common;

namespace XgsPon.Workflows.Client.Model.Response
{
    public class GetAllWorkflowDefinitionsResponse
    {
        [JsonProperty("result")]
        public List<WorkflowDefinition> Result { get; set; }
    }
}
