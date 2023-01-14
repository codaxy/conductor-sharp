using ConductorSharp.Proxy.Models;

namespace ConductorSharp.Proxy.Services
{
    public interface IPolledWokflowRegistry
    {
        void Poll(string workflowId);
        void StopPolling(string workflowId);
        ICollection<string> GetPolledWorkflows();
        Task PublishUpdate(TaskPollResult pollResult);
    }
}
