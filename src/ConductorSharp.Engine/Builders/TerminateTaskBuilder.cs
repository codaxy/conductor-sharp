using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.Builders
{
    internal class TerminateTaskBuilder : BaseTaskBuilder<TerminateTaskInput, NoOutput>
    {
        public TerminateTaskBuilder(Expression taskExpression, Expression memberExpression) : base(taskExpression, memberExpression) { }

        public override WorkflowDefinition.Task[] Build() =>
            new[]
            {
                new WorkflowDefinition.Task
                {
                    Name = $"TERMINATE_{_taskRefferenceName}",
                    TaskReferenceName = _taskRefferenceName,
                    Type = "TERMINATE",
                    InputParameters = _inputParameters,
                    Description = new JObject { new JProperty("description", _description) }.ToString(Newtonsoft.Json.Formatting.None),
                }
            };
    }
}
