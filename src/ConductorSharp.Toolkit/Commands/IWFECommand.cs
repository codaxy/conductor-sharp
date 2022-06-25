using ConductorSharp.Toolkit.Models;

namespace ConductorSharp.Toolkit.Commands
{
    public interface IWFECommand
    {
        public string GetName();
        public Task Execute(WFECommandInput input);
    }
}
