using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json;

namespace ConductorSharp.Engine.Model
{
    public abstract class TaskModel<I, O> : ITaskModel where I : IRequest<O>
    {
        public I Input { get; }

        public O Output { get; }

        public string Status { get; }

        [PropertyName("taskType")]
        public string TaskType { get; }

        [PropertyName("startTime")]
        public long StartTime { get; }

        [PropertyName("endTime")]
        public long EndTime { get; }

        [PropertyName("taskId")]
        public string TaskId { get; }

        [PropertyName("taskDefName")]
        public string TaskDefName { get; }

        [PropertyName("scheduledTime")]
        public long ScheduledTime { get; }

        [PropertyName("referenceTaskName")]
        public string ReferenceTaskName { get; }

        [PropertyName("correlationId")]
        public string CorrelationId { get; }
    }
}
