using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Model
{
    public class WaitTaskInput : IRequest<NoOutput>
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("until")]
        public string Until { get; set; }
    }

    public class WaitTaskModel : TaskModel<WaitTaskInput, NoOutput> { }
}
