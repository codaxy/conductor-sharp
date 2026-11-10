using Newtonsoft.Json;

namespace ConductorSharp.Client.Generated
{
    // The execution context is a Maestro extension of the Conductor API: key/value pairs a workflow
    // run is started with, kept unchanged for the life of the run and inherited by its sub-workflows.
    // Conductor neither sends nor reads it. These partials live outside Models.cs so regenerating the
    // models does not drop them.

    public partial class Task
    {
        /// <summary>
        /// Execution context of the workflow run this task belongs to, as Maestro sends it with a
        /// polled task. Null from Conductor.
        /// </summary>
        [JsonProperty("context", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string>? Context { get; set; }
    }

    public partial class Workflow
    {
        /// <summary>
        /// Execution context the run was started with, as Maestro reports it on a workflow read.
        /// Null from Conductor.
        /// </summary>
        [JsonProperty("context", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string>? Context { get; set; }
    }

    public partial class StartWorkflowRequest
    {
        /// <summary>
        /// Execution context to start the run with. Maestro stores it with the run, hands it to every
        /// task and sub-workflow, and never lets the workflow change it. Omitted from the request when
        /// null, so a start against Conductor is unchanged.
        /// </summary>
        [JsonProperty("context", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string>? Context { get; set; }
    }
}
