using Newtonsoft.Json;
using System.Collections.Generic;
using XgsPon.Workflows.Client.Model.Common;

namespace XgsPon.Workflows.Client.Model.Response
{
    public class GetAllTaskDefinitionsResponse
    {
        [JsonProperty("result")]
        public List<TaskDefinition> Result { get; set; }
    }
}
