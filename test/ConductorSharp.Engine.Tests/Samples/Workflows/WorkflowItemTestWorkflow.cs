using ConductorSharp.Engine.Builders.Configurable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class WorkflowItemTestWorkflowInput : WorkflowInput<WorkflowItemTestWorkflowOutput> { }

    public class WorkflowItemTestWorkflowOutput : WorkflowOutput { }

    public class WorkflowItemTestWorkflow : Workflow<WorkflowItemTestWorkflow, WorkflowItemTestWorkflowInput, WorkflowItemTestWorkflowOutput>
    {
        public const string TestKey = "test";
        public const string TestValue = "123";

        public WorkflowItemTestWorkflow(
            WorkflowDefinitionBuilder<WorkflowItemTestWorkflow, WorkflowItemTestWorkflowInput, WorkflowItemTestWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.WorkflowBuildRegistry.Register<WorkflowItemTestWorkflow>(TestKey, TestValue);
        }
    }
}
