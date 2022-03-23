using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XgsPon.Workflows.Client.Model.Response
{
    public class ConductorValidationError
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
