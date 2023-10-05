﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class VariableAccessWorkflowInput : WorkflowInput<VariableAccessWorkflowOutput> { }

    public class VariableAccessWorkflowOutput : WorkflowOutput { }

    public class VariableAccessWorkflow : Workflow<VariableAccessWorkflow, VariableAccessWorkflowInput, VariableAccessWorkflowOutput>
    {
        public CustomerGetV1 FirstCustomerGet { get; set; }
        public CustomerGetV1 SecondCustomerGet { get; set; }
        public CustomerGetV1 ThirdCustomerGet { get; set; }
        public EmailPrepareV1 PrepareEmail { get; set; }

        public static int StaticVar = int.Parse("1");
        public static int StaticProp { get; set; } = int.Parse("1");

        public VariableAccessWorkflow(
            WorkflowDefinitionBuilder<VariableAccessWorkflow, VariableAccessWorkflowInput, VariableAccessWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            int myVar = int.Parse("1");

            _builder.AddTask(wf => wf.FirstCustomerGet, wf => new() { CustomerId = myVar });
            _builder.AddTask(wf => wf.SecondCustomerGet, wf => new() { CustomerId = StaticVar });

            _builder.AddTask(wf => wf.ThirdCustomerGet, wf => new() { CustomerId = StaticProp });
        }
    }
}
