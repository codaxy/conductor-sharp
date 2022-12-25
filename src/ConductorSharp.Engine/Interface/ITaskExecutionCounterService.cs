using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Interface
{
    public class RunningTask
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTimeOffset StartedAt { get; set; }
    }

    public class TaskRunCount
    {
        public string TaskName { get; set; }
        public int Count { get; set; }
    }

    public interface ITaskExecutionCounterService
    {
        void Track(RunningTask trackedTask);

        void MoveToFailed(string taskId);

        void MoveToCompleted(string taskId);

        List<TaskRunCount> GetCompletedCount();
        List<TaskRunCount> GetFailedCount();
        List<RunningTask> GetRunning();
    }
}
