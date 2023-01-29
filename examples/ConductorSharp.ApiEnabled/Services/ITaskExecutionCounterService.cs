using ConductorSharp.Engine.Interface;

namespace ConductorSharp.ApiEnabled.Services
{
    public class TaskRunCount
    {
        public string TaskName { get; set; }
        public int Count { get; set; }
    }

    public interface ITaskExecutionCounterService : ITaskExecutionService
    {
        List<TaskRunCount> GetCompletedCount();
        List<TaskRunCount> GetFailedCount();
        List<RunningTask> GetRunning();
    }
}
