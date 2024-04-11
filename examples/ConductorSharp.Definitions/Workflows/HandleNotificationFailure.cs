using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Builders.Metadata;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Tasks;
using Newtonsoft.Json;

namespace ConductorSharp.Definitions.Workflows
{
    public class HandleNotificationFailureInput : WorkflowInput<HandleNotificationFailureOutput>
    {
        [JsonProperty("workflowId")]
        public string WorkflowId { get; set; }
    }

    public class HandleNotificationFailureOutput : WorkflowOutput { }

    [OriginalName("NOTIFICATION_handle_failure")]
    [WorkflowMetadata(OwnerEmail = "test@test.com")]
    public class HandleNotificationFailure : Workflow<HandleNotificationFailure, HandleNotificationFailureInput, HandleNotificationFailureOutput>
    {
        public HandleNotificationFailure(
            WorkflowDefinitionBuilder<HandleNotificationFailure, HandleNotificationFailureInput, HandleNotificationFailureOutput> builder
        )
            : base(builder) { }

        public ReadWorkflowTasks ReadExecutedTasks { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(a => a.ReadExecutedTasks, b => new() { TaskNames = "dynamic_handler", WorkflowId = b.WorkflowInput.WorkflowId });
        }
    }
}
