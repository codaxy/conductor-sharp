using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConductorSharp.Engine.Service
{

    public class DeploymentValidator
    {
        public class Result
        {
            public List<string> Errors { get; set; }
            public List<string> Warnings { get; set; }
        }

        public Result Validate(Deployment deployment)
        {
            var result = new Result { Errors = new List<string>(), Warnings = new List<string>() };

            foreach (var workflow in deployment.WorkflowDefinitions)
            {
                ValidateWorkflow(workflow, deployment, result);
            }

            return result;
        }

        void ValidateWorkflow(
            WorkflowDefinition workflowDefinition,
            Deployment deployment,
            Result result
        )
        {
            foreach (var task in workflowDefinition.Tasks)
            {
                var def = deployment.FindTaskByName(task.Name);
                if (def == null)
                {
                    result.Warnings.Add(
                        $"Task {task.Name} cannot be validated as its definition is not a part of the deployment."
                    );
                    continue;
                }
                foreach (var pair in task.InputParameters)
                {
                    if (pair.Value.Type != JTokenType.String)
                        continue;
                    var value = (string)pair.Value;
                    if (value.StartsWith("${") && value.EndsWith("}"))
                    {
                        ValidateTaskInputParameterBinding(
                            value,
                            task.Name,
                            workflowDefinition,
                            deployment,
                            result
                        );
                    }
                    else
                    {
                        result.Errors.Add(
                            $"Task {task.Name} parameter {pair.Key} is not a binding. Value: {value}"
                        );
                    }
                }
            }
        }

        private void ValidateTaskInputParameterBinding(
            string binding,
            string parentTaskName,
            WorkflowDefinition workflowDefinition,
            Deployment deployment,
            Result result
        )
        {
            var parts = binding.Substring(2, binding.Length - 3).Split('.');

            var taskReferenceName = parts[0];
            var inOut = parts[1];
            var paramName = parts[2];

            if (inOut != "input" && inOut != "output")
            {
                result.Errors.Add(
                    $"The input parameter binding {binding} in task {parentTaskName} is not valid. Please reference either input or output parameters. Got: {inOut}."
                );
                return;
            }

            if (taskReferenceName == "workflow")
            {
                if (inOut == "input" && !workflowDefinition.InputParameters.ContainsKey(paramName))
                    result.Errors.Add(
                        $"The parameter binding {binding} in task {parentTaskName} is not valid. The workflow does not contain the input parameter {paramName}."
                    );
                if (
                    inOut == "output" && !workflowDefinition.OutputParameters.ContainsKey(paramName)
                )
                    result.Errors.Add(
                        $"The parameter binding {binding} in task {parentTaskName} is not valid. The workflow does not contain the output parameter {paramName}."
                    );
            }
            else
            {
                var task = workflowDefinition.Tasks.Find(
                    t => t.TaskReferenceName == taskReferenceName
                );
                if (task == null)
                {
                    result.Errors.Add(
                        $"The parameter binding {binding} in task {parentTaskName} references a task {taskReferenceName} which cannot be found in the workflow."
                    );
                    return;
                }
                var taskDefinition = deployment.FindTaskByName(task.Name);
                if (taskDefinition == null)
                {
                    result.Warnings.Add(
                        $"The parameter binding {binding} in task {parentTaskName} cannot be validated as it references a task {taskReferenceName} which is not part of the same deployment."
                    );
                    return;
                }

                if (inOut == "input" && !taskDefinition.InputKeys.Contains(paramName))
                    result.Errors.Add(
                        $"The parameter binding {binding} in task {parentTaskName} is not valid. The task {taskDefinition.Name} does not contain the input parameter {paramName}."
                    );

                if (inOut == "output" && !taskDefinition.OutputKeys.Contains(paramName))
                    result.Errors.Add(
                        $"The parameter binding {binding} in task {parentTaskName} is not valid. The task {taskDefinition.Name} does not contain the output parameter {paramName}."
                    );
            }
        }
    }
}