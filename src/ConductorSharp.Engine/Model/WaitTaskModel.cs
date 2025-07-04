using ConductorSharp.Engine.Interface;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public class WaitTaskInput : ITaskInput<NoOutput>
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("until")]
        public string Until { get; set; }
    }

    public class WaitTaskModel : TaskModel<WaitTaskInput, NoOutput>;
}
