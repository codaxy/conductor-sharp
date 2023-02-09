﻿using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Definitions.Workflows
{
    public class CSharpLambdaWorkflowInput : WorkflowInput<CSharpLambdaWorkflowOutput>
    {
        public string Operation { get; set; }
        public string Input { get; set; }
    }

    public class CSharpLambdaWorkflowOutput : WorkflowOutput { }

    public class CSharpLambdaWorkflow : Workflow<CSharpLambdaWorkflow, CSharpLambdaWorkflowInput, CSharpLambdaWorkflowOutput>
    {
        public class LambdaTaskInput : IRequest<LambdaTaskOutput>
        {
            public string LambdaInput { get; set; }
        }

        public class LambdaTaskOutput
        {
            public string LambdaOutput { get; set; }
        }

        public CSharpLambdaTaskModel<LambdaTaskInput, LambdaTaskOutput> FirstLambdaTask { get; set; }
        public CSharpLambdaTaskModel<LambdaTaskInput, LambdaTaskOutput> SecondLambdaTask { get; set; }
        public CSharpLambdaTaskModel<LambdaTaskInput, LambdaTaskOutput> ThirdLambdaTask { get; set; }
        public DecisionTaskModel DecisionTask { get; set; }

        public CSharpLambdaWorkflow(WorkflowDefinitionBuilder<CSharpLambdaWorkflow, CSharpLambdaWorkflowInput, CSharpLambdaWorkflowOutput> builder)
            : base(builder) { }

        public override void BuildDefinition()
        {
            _builder.AddTask(
                wf => wf.FirstLambdaTask,
                wf => new() { LambdaInput = wf.WorkflowInput.Input },
                input =>
                {
                    string output = input.LambdaInput;

                    if (input.LambdaInput.Length < 5)
                        output = input.LambdaInput.PadRight(5, 'a');

                    return new() { LambdaOutput = output };
                }
            );

            _builder.AddTask(
                wf => wf.DecisionTask,
                wf => new DecisionTaskInput() { CaseValueParam = wf.WorkflowInput.Operation },
                (
                    "upper",
                    builder =>
                    {
                        builder.WithTask(
                            wf => wf.SecondLambdaTask,
                            wf => new() { LambdaInput = wf.WorkflowInput.Input },
                            input =>
                            {
                                return new() { LambdaOutput = new string(input.LambdaInput.ToUpperInvariant()) };
                            }
                        );
                    }
                ),
                (
                    "lower",
                    builder =>
                    {
                        builder.WithTask(
                            wf => wf.ThirdLambdaTask,
                            wf => new() { LambdaInput = wf.WorkflowInput.Input },
                            input =>
                            {
                                return new() { LambdaOutput = new string(input.LambdaInput.ToLowerInvariant()) };
                            }
                        );
                    }
                )
            );

            _builder.SetOptions(options => options.OwnerEmail = "test@test.com");
        }
    }
}
