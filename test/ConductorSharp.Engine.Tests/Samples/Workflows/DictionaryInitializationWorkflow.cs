﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class DictionaryInitializationWorkflowInput : WorkflowInput<DictionaryInitializationWorkflowOutput> { }

    public class DictionaryInitializationWorkflowOutput : WorkflowOutput { }

    public class DictionaryInitializationWorkflow
        : Workflow<DictionaryInitializationWorkflow, DictionaryInitializationWorkflowInput, DictionaryInitializationWorkflowOutput>
    {
        public DictionaryInitializationWorkflow(
            WorkflowDefinitionBuilder<
                DictionaryInitializationWorkflow,
                DictionaryInitializationWorkflowInput,
                DictionaryInitializationWorkflowOutput
            > builder
        )
            : base(builder) { }

        public DictionaryInputTask DictionaryTask { get; set; }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.DictionaryTask,
                wf =>
                    new()
                    {
                        Input = new Dictionary<string, object>() { { "test1", "value" }, { "test2", new { MyProp = "test" } } }
                    }
            );
        }
    }
}