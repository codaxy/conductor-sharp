using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Tests.Samples.Workers;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class IndexerWorkflowInput : WorkflowInput<IndexerWorkflowOutput>
    {
        public class TestModel
        {
            public string CustomerName { get; set; }
        }

        public Dictionary<string, TestModel> Dictionary { get; set; }
        public Dictionary<string, Dictionary<string, string>> DoubleDictionary { get; set; }
    }

    public class IndexerWorkflowOutput : WorkflowOutput { }

    public class IndexerWorkflow : Workflow<IndexerWorkflow, IndexerWorkflowInput, IndexerWorkflowOutput>
    {
        public PrepareEmailHandler PrepareEmail { get; set; }

        public IndexerWorkflow(WorkflowDefinitionBuilder<IndexerWorkflow, IndexerWorkflowInput, IndexerWorkflowOutput> builder) : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.PrepareEmail,
                wf =>
                    new()
                    {
                        CustomerName = wf.WorkflowInput.Dictionary["test"].CustomerName,
                        Address = wf.WorkflowInput.DoubleDictionary["test"]["address"]
                    }
            );
        }
    }
}
