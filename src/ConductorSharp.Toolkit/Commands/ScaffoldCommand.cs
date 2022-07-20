using ConductorSharp.Toolkit.Models;
using ConductorSharp.Toolkit.Service;

namespace ConductorSharp.Toolkit.Commands
{
    public class ScaffoldCommand : Command
    {
        private readonly IScaffoldingService _scaffoldingService;

        public ScaffoldCommand(IScaffoldingService scaffoldingService)
        {
            _scaffoldingService = scaffoldingService;
        }

        public string GetName() => "scaffold";

        public async Task Execute(CommandInput input)
        {
            await _scaffoldingService.Scaffold();
        }
    }
}
