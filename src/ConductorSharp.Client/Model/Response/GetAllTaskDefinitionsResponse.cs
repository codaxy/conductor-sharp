using ConductorSharp.Client.Model.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Response
{
    public class GetAllTaskDefinitionsResponse
    {
        [JsonProperty("result")]
        public List<TaskDefinition> Result { get; set; }
    }
}
