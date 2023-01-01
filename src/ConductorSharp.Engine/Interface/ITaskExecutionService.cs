using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface
{
    public class RunningTask
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTimeOffset StartedAt { get; set; }
    }

    public interface ITaskExecutionService
    {
        Task OnPolled(RunningTask runningTask);
        Task OnCompleted(RunningTask runningTask);
        Task OnFailed(RunningTask runningTask);
    }
}
