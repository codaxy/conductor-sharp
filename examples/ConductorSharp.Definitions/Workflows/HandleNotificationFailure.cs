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
        [PropertyName("workflowId")]
        public string WorkflowId { get; set; }
    }

    public class HandleNotificationFailureOutput : WorkflowOutput { }

    [OriginalName("NOTIFICATION_handle_failure")]
    public class HandleNotificationFailure : Workflow<HandleNotificationFailure, HandleNotificationFailureInput, HandleNotificationFailureOutput>
    {
        public HandleNotificationFailure(
            WorkflowDefinitionBuilder<HandleNotificationFailure, HandleNotificationFailureInput, HandleNotificationFailureOutput> builder
        ) : base(builder) { }

        public ReadWorkflowTasks ReadExecutedTasks { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(a => a.ReadExecutedTasks, b => new() { TaskNames = "dynamic_handler", WorkflowId = b.WorkflowInput.WorkflowId });
            _builder.SetOptions(options => options.OwnerEmail = "test@test.com");
        }
    }
}
