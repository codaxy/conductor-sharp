using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Builders
{
    public class DynamicForkJoinTaskBuilder : BaseTaskBuilder<DynamicForkJoinInput, NoOutput>
    {
        public DynamicForkJoinTaskBuilder(Expression taskExpression, Expression inputExpression) : base(taskExpression, inputExpression) { }

        public override WorkflowDefinition.Task[] Build()
        {
            var dynamicTaskName = $"FORK_JOIN_DYNAMIC_{_taskRefferenceName}";
            var joinTaskName = $"JOIN_{_taskRefferenceName}";

            return new WorkflowDefinition.Task[]
            {
                new WorkflowDefinition.Task
                {
                    Name = dynamicTaskName,
                    TaskReferenceName = dynamicTaskName,
                    Type = "FORK_JOIN_DYNAMIC",
                    DynamicForkTasksParam = "dynamic_tasks",
                    DynamicForkTasksInputParamName = "dynamic_tasks_i",
                    InputParameters = _inputParameters,
                    Description = new JObject { new JProperty("description", "A dynamic fork.") }.ToString(Newtonsoft.Json.Formatting.None)
                },
                new WorkflowDefinition.Task
                {
                    Name = joinTaskName,
                    TaskReferenceName = joinTaskName,
                    Type = "JOIN",
                    Description = new JObject { new JProperty("description", "Join.") }.ToString(Newtonsoft.Json.Formatting.None)
                }
            };
        }
    }
}
