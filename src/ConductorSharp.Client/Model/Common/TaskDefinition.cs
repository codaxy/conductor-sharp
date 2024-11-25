using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConductorSharp.Client.Model.Common
{
    public class TaskDefinition
    {
        [JsonProperty("ownerApp")]
        public string OwnerApp { get; set; } = "test@test.local";

        [JsonProperty("createTime")]
        public long? CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public long? UpdateTime { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updatedBy")]
        public string UpdatedBy { get; set; }

        /// <summary>Task Type. Unique name of the Task that resonates with it's function.</summary>
        /// <remarks>Unique</remarks>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>Description of the task.</summary>
        /// <remarks>Optional</remarks>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>No. of retries to attempt when a Task is marked as failure.</summary>
        /// <remarks>Defaults to 3</remarks>
        [JsonProperty("retryCount")]
        public int RetryCount { get; set; }

        /// <summary>Time in seconds, after which the task is marked as TIMED_OUT if not completed after transitioning to IN_PROGRESS status for the first time.</summary>
        /// <remarks>No timeouts if set to 0</remarks>
        [JsonProperty("timeoutSeconds")]
        public int TimeoutSeconds { get; set; }

        /// <summary>Array of keys of task's expected input. Used for documenting task's inpu.</summary>
        /// <remarks>Optional</remarks>
        [JsonProperty("inputKeys")]
        public List<string> InputKeys { get; set; }

        /// <summary>Array of keys of task's expected output. Used for documenting task's output.</summary>
        /// <remarks>Optional</remarks>
        [JsonProperty("outputKeys")]
        public List<string> OutputKeys { get; set; }

        /// <summary>Task's timeout policy.</summary>
        /// <remarks>Values should be assigned from the TimeoutPolicy constants class</remarks>
        [JsonProperty("timeoutPolicy")]
        public string TimeoutPolicy { get; set; }

        /// <summary>Mechanism for the retries.</summary>
        /// <remarks>Values should be assigned from the RetryLogic constants class</remarks>
        [JsonProperty("retryLogic")]
        public string RetryLogic { get; set; }

        /// <summary>Time to wait before retries.</summary>
        /// <remarks>Defaults to 60 seconds</remarks>
        [JsonProperty("retryDelaySeconds")]
        public int RetryDelaySeconds { get; set; }

        /// <summary>The task is rescheduled if not updated with a status after this time (heartbeat mechanism). Useful when the worker polls for the task but fails to complete due to errors/network failure.</summary>
        /// <remarks>Must be greater than 0 and less than timeoutSeconds, defaults to 3600</remarks>
        [JsonProperty("responseTimeoutSeconds")]
        public int ResponseTimeoutSeconds { get; set; }

        /// <summary>Number of tasks that can be executed at any given time.</summary>
        /// <remarks>Optional</remarks>
        [JsonProperty("concurrentExecLimit")]
        public int ConcurrentExecLimit { get; set; }

        /// <summary>Allows to define default values, which can be overridden by values provided in Workflow.</summary>
        [JsonProperty("inputTemplate")]
        public JObject InputTemplate { get; set; }

        /// <summary>Defines the number of Tasks that can be given to Workers per given "frequency window".</summary>
        /// <remarks>RateLimitFrequencyInSeconds and RateLimitPerFrequency should be used together.</remarks>
        [JsonProperty("rateLimitPerFrequency")]
        public int RateLimitPerFrequency { get; set; }

        /// <summary>Sets the "frequency window", i.e the duration to be used in events per duration. Eg: 1s, 5s, 60s, 300s etc.</summary>
        /// <remarks>RateLimitFrequencyInSeconds and RateLimitPerFrequency should be used together.</remarks>
        [JsonProperty("rateLimitFrequencyInSeconds")]
        public int RateLimitFrequencyInSeconds { get; set; }

        /// <summary>When we set isolationGroupId, the executor(SystemTaskWorkerCoordinator) will allocate an isolated queue and an isolated thread pool for execution of those tasks.</summary>
        /// <remarks>IsolationGroupId is currently supported only in HTTP and kafka Task.</remarks>
        [JsonProperty("isolationGroupId")]
        public string IsolationGroupId { get; set; }

        /// <summary>To support JVM isolation, and also allow the executors to scale horizontally, we can use executionNameSpace property in taskdef.</summary>
        /// <remarks>ExecutionNameSpace can be used along with isolationGroupId</remarks>
        [JsonProperty("executionNameSpace")]
        public string ExecutionNameSpace { get; set; }

        [JsonProperty("ownerEmail")]
        public string OwnerEmail { get; set; }

        ///<summary>Time in seconds, after which the task is marked as TIMED_OUT if not polled by a worker.</summary>
        ///<remarks>No timeouts if set to 0</remarks>
        [JsonProperty("pollTimeoutSeconds")]
        public int PollTimeoutSeconds { get; set; }

        public static bool AreEqual(TaskDefinition t1, TaskDefinition t2)
        {
            var o1 = JsonConvert.SerializeObject(t1);
            var o2 = JsonConvert.SerializeObject(t2);

            return o1.Equals(o2);
        }
    }
}
