namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class TaskPropertiesTaskInput : IRequest<TaskPropertiesTaskOutput>
    {
        public string Status { get; set; }
        public string TaskType { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string TaskId { get; set; }
        public string TaskDefName { get; set; }
        public long ScheduledTime { get; set; }
        public string ReferenceTaskName { get; set; }
    }

    public class TaskPropertiesTaskOutput { }

    public class TaskPropertiesTask : SimpleTaskModel<TaskPropertiesTaskInput, TaskPropertiesTaskOutput> { }
}
