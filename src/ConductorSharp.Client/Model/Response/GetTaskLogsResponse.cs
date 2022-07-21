using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ConductorSharp.Client.Model.Response
{
    public class GetTaskLogsResponse
    {
        public string Log { get; set; }
        public Guid TaskId { get; set; }

        [JsonProperty("createdTime")]
        public long CreatedTimeTimestamp { get; set; }
    }
}
