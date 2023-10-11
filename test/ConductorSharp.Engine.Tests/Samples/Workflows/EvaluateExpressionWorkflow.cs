using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Workflows
{
    public class EvaluateExpressionWorkflowInput : WorkflowInput<EvaluateExpressionWorkflowOutput>
    {
        public string Username { get; set; }
    }

    public class EvaluateExpressionWorkflowOutput : WorkflowOutput { }

    public class EvaluateExpressionWorkflow : Workflow<EvaluateExpressionWorkflow, EvaluateExpressionWorkflowInput, EvaluateExpressionWorkflowOutput>
    {
        public class TestModel
        {
            public int Int { get; set; } = 1;
            public Dictionary<string, int> Dict { get; set; } = new() { { "key", 1 } };
        }

        public CustomerGetV1 FirstCustomerGet { get; set; }
        public CustomerGetV1 SecondCustomerGet { get; set; }
        public CustomerGetV1 ThirdCustomerGet { get; set; }
        public CustomerGetV1 FourthCustomerGet { get; set; }
        public CustomerGetV1 FifthCustomerGet { get; set; }
        public EmailPrepareV1 PrepareEmail { get; set; }

        public static int StaticVar = int.Parse("1");
        public static int StaticProp { get; set; } = int.Parse("1");
        public TestModel ClassProp { get; set; } = new();

        public EvaluateExpressionWorkflow(
            WorkflowDefinitionBuilder<EvaluateExpressionWorkflow, EvaluateExpressionWorkflowInput, EvaluateExpressionWorkflowOutput> builder
        ) : base(builder) { }

        public override void BuildDefinition()
        {
            var localVar = int.Parse("1");

            _builder.AddTask(wf => wf.FirstCustomerGet, wf => new() { CustomerId = localVar });
            _builder.AddTask(wf => wf.SecondCustomerGet, wf => new() { CustomerId = StaticVar });
            _builder.AddTask(wf => wf.ThirdCustomerGet, wf => new() { CustomerId = StaticProp });
            _builder.AddTask(wf => wf.FourthCustomerGet, wf => new() { CustomerId = ClassProp.Int });
            _builder.AddTask(wf => wf.FifthCustomerGet, wf => new() { CustomerId = Fibonacci(10) + Fibonacci(1) });
            _builder.AddTask(
                wf => wf.PrepareEmail,
                wf =>
                    new()
                    {
                        Name = $"{localVar}:{StaticVar}:{StaticProp}:{ClassProp.Int}:{ClassProp.Dict["key"]}",
                        Address = wf.WorkflowInput.Username + $"@{FetchDomainFromDb()}".ToLowerInvariant()
                    }
            );
        }

        private string FetchDomainFromDb() => "TEST.COM";

        private int Fibonacci(int n)
        {
            if (n == 1)
                return 0;
            if (n == 2)
                return 1;
            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }
    }
}
