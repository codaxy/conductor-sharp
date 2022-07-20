using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Response
{
    public class HealthResponse
    {
        public class HealthResult
        {
            [JsonProperty("details")]
            public JObject Details { get; set; }

            [JsonProperty("healthy")]
            public bool Healthy { get; set; }

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }
        }

        public class SuppressedHealthResult
        {
            [JsonProperty("details")]
            public JObject Details { get; set; }

            [JsonProperty("healthy")]
            public bool Healthy { get; set; }

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }
        }

        [JsonProperty("healthResults")]
        public List<HealthResult> HealthResults { get; set; }

        [JsonProperty("suppressedHealthResults")]
        public List<SuppressedHealthResult> SuppressedHealthResults { get; set; }

        [JsonProperty("healthy")]
        public bool Healthy { get; set; }
    }
}
