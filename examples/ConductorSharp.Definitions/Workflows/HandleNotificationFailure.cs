using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Definitions.Workflows
{
    public class HandleNotificationFailureInput : WorkflowInput<HandleNotificationFailureOutput>
    {
        [JsonProperty("workflowId")]
        public string? WorkflowId { get; set; }
    }

    public class HandleNotificationFailureOutput : WorkflowOutput { }

    [OriginalName("NOTIFICATION_handle_failure")]
    public class HandleNotificationFailure : Workflow<HandleNotificationFailureInput, HandleNotificationFailureOutput>
    {
        public ReadWorkflowTasks? ReadExecutedTasks { get; set; }

        public override WorkflowDefinition GetDefinition()
        {
            var builder = new WorkflowDefinitionBuilder<HandleNotificationFailure>();

            builder.AddTask(a => a.ReadExecutedTasks, b => new() { TaskNames = "dynamic_handler", WorkflowId = b.WorkflowInput.WorkflowId });

            return builder.Build(options =>
            {
                options.Version = 1;
                options.OwnerEmail = "example@example.local";
            });
        }
    }
}
