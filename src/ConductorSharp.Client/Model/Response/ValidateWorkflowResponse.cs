using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class ValidateWorkflowResponse
    {
        public class ValidationErrors
        {
            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("retryable")]
        public bool Retryable { get; set; }

        public ValidationErrors[] ValidationErrorsResponse { get; set; }
    }
}
