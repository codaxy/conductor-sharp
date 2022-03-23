using Autofac;
using Newtonsoft.Json.Linq;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Builders;

namespace XgsPon.WorkflowDefinisions
{
    public class TestWorkflowInput : WorkflowInput<TestWorkflowOutput>
    {
    }

    public class TestWorkflowOutput : WorkflowOutput
    {
    }

    public class TestWorkflow : Workflow<TestWorkflowInput, TestWorkflowOutput>
    {
        public override WorkflowDefinition GetDefinition() =>
            new()
            {
                Name = "TEST_workflow",
                OwnerEmail = "test@codaxy.com",
                Description = new JObject()
                {
                    new JProperty("description", "Test Workflow"),
                    new JProperty("labels", new string[] { "Test Workflow" })
                }.ToString(Newtonsoft.Json.Formatting.None),
                OwnerApp = "XgsPon.Workflow",
                InputParameters = new JObject
                {
                },
                SchemaVersion = 2,
                Restartable = true,
                WorkflowStatusListenerEnabled = true,
                Tasks = new List<WorkflowDefinition.Task>
                {
                    new WorkflowDefinition.Task
                    {
                        Name = "TEST_worker",
                        TaskReferenceName = "test_worker",
                        InputParameters = new JObject { },
                        Type = "SIMPLE",
                        StartDelay = 0,
                        Optional = false,
                        AsyncComplete = false,
                        Description = new JObject { new JProperty("description", "Test worker") }.ToString(
                            Newtonsoft.Json.Formatting.None
                        )
                    },
                }
            };
    }
}
