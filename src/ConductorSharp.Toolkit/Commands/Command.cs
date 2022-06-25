using ConductorSharp.Toolkit.Models;

namespace ConductorSharp.Toolkit.Commands
{
    public interface Command
    {
        public string GetName();
        public Task Execute(WFECommandInput input);
    }
}
