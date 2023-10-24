using ConductorSharp.Engine.Interface;
using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public abstract class TaskModel<I, O> : ITaskModel where I : IRequest<O>
    {
        public I Input { get; }

        public O Output { get; }

        public string Status { get; }

        [JsonProperty("taskType")]
        public string TaskType { get; }

        [JsonProperty("startTime")]
        public long StartTime { get; }

        [JsonProperty("endTime")]
        public long EndTime { get; }

        [JsonProperty("taskId")]
        public string TaskId { get; }

        [JsonProperty("taskDefName")]
        public string TaskDefName { get; }

        [JsonProperty("scheduledTime")]
        public long ScheduledTime { get; }

        [JsonProperty("referenceTaskName")]
        public string ReferenceTaskName { get; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; }
    }
}
