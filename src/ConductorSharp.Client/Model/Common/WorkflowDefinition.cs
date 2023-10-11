using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

//using System.Text.Json.Serialization;

namespace ConductorSharp.Client.Model.Common
{
    public class WorkflowDefinition
    {
        public class SubWorkflowParam
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("taskToDomain")]
            public JObject TaskToDomain { get; set; }

            [JsonProperty("workflowDefinition")]
            public WorkflowDefinition WorkflowDefinition { get; set; }
        }

        public class Task
        {
            private JObject inputParameters;

            [JsonIgnore]
            public string ResumableTaskName { get; set; }

            [JsonIgnore]
            public string ResumableTaskCramerServiceId { get; set; }

            [JsonIgnore]
            public string ResumableTaskType { get; set; }

            [JsonProperty("queryExpression")]
            public string QueryExpression { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("taskReferenceName")]
            public string TaskReferenceName { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("inputParameters")]
            public JObject InputParameters
            {
                set => inputParameters = value;
                get
                {
                    if (
                        !string.IsNullOrEmpty(ResumableTaskType)
                        || !string.IsNullOrEmpty(ResumableTaskName)
                        || !string.IsNullOrEmpty(ResumableTaskCramerServiceId)
                    )
                        inputParameters ??= new JObject();
                    else
                        return inputParameters;

                    if (!inputParameters.ContainsKey("task_type") && !string.IsNullOrEmpty(ResumableTaskType))
                        inputParameters.Add(new JProperty("task_type", ResumableTaskType));

                    if (!inputParameters.ContainsKey("resumable_task_cramer_service_id") && !string.IsNullOrEmpty(ResumableTaskCramerServiceId))
                        inputParameters.Add(new JProperty("resumable_task_cramer_service_id", ResumableTaskCramerServiceId));

                    if (!inputParameters.ContainsKey("resumable_task_name") && !string.IsNullOrEmpty(ResumableTaskName))
                        inputParameters.Add(new JProperty("resumable_task_name", ResumableTaskName));

                    return inputParameters;
                }
            }

            /// <summary>
            /// default: SIMPLE
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; set; } = ConductorConstants.SimpleTask;

            [JsonProperty("dynamicTaskNameParam")]
            public string DynamicTaskNameParam { get; set; }

            [JsonProperty("caseValueParam")]
            public string CaseValueParam { get; set; }

            [JsonProperty("caseExpression")]
            public string CaseExpression { get; set; }

            [JsonProperty("expression")]
            public string Expression { get; set; }

            [JsonProperty("evaluatorType")]
            public string EvaluatorType { get; set; }

            [JsonProperty("scriptExpression")]
            public string ScriptExpression { get; set; }

            [JsonProperty("decisionCases")]
            public JObject DecisionCases { get; set; }

            [JsonProperty("dynamicForkJoinTasksParam")]
            public string DynamicForkJoinTasksParam { get; set; }

            [JsonProperty("dynamicForkTasksParam")]
            public string DynamicForkTasksParam { get; set; }

            [JsonProperty("dynamicForkTasksInputParamName")]
            public string DynamicForkTasksInputParamName { get; set; }

            [JsonProperty("defaultCase")]
            public List<JObject> DefaultCase { get; set; }

            [JsonProperty("forkTasks")]
            public List<List<JObject>> ForkTasks { get; set; }

            [JsonProperty("startDelay")]
            public int StartDelay { get; set; }

            [JsonProperty("subWorkflowParam")]
            public SubWorkflowParam SubWorkflowParam { get; set; }

            [JsonProperty("joinOn")]
            public List<string> JoinOn { get; set; }

            [JsonProperty("sink")]
            public string Sink { get; set; }

            [JsonProperty("optional")]
            public bool Optional { get; set; }

            [JsonProperty("taskDefinition")]
            public TaskDefinition TaskDefinition { get; set; }

            [JsonProperty("rateLimited")]
            public bool RateLimited { get; set; }

            [JsonProperty("defaultExclusiveJoinTask")]
            public List<string> DefaultExclusiveJoinTask { get; set; }

            [JsonProperty("asyncComplete")]
            public bool AsyncComplete { get; set; }

            [JsonProperty("loopCondition")]
            public string LoopCondition { get; set; }

            [JsonProperty("loopOver")]
            public List<JObject> LoopOver { get; set; }
        }

        [JsonProperty("ownerApp")]
        public string OwnerApp { get; set; }

        [JsonProperty("createTime")]
        public long CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// default: 1
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonProperty("tasks")]
        public List<Task> Tasks { get; set; }

        [JsonProperty("inputParameters")]
        public string[] InputParameters { get; set; }

        [JsonProperty("outputParameters")]
        public JObject OutputParameters { get; set; }

        [JsonProperty("failureWorkflow")]
        public string FailureWorkflow { get; set; }

        /// <summary>
        /// default: 2
        /// </summary>
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; } = 2;

        /// <summary>
        /// default: true
        /// </summary>
        [JsonProperty("restartable")]
        public bool Restartable { get; set; } = true;

        /// <summary>
        /// default: true
        /// </summary>
        [JsonProperty("workflowStatusListenerEnabled")]
        public bool WorkflowStatusListenerEnabled { get; set; } = true;

        [JsonProperty("ownerEmail")]
        public string OwnerEmail { get; set; }

        [JsonProperty("timeoutPolicy")]
        public string TimeoutPolicy { get; set; }

        [JsonProperty("timeoutSeconds")]
        public int TimeoutSeconds { get; set; }

        [JsonProperty("variables")]
        public JObject Variables { get; set; }

        public static bool AreEqual(WorkflowDefinition d1, WorkflowDefinition d2)
        {
            var o1 = JsonConvert.SerializeObject(d1);
            var o2 = JsonConvert.SerializeObject(d2);

            return o1.Equals(o2);
        }
    }
}
