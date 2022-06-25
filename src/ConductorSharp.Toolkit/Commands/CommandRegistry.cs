namespace ConductorSharp.Toolkit.Commands
{
    public class CommandRegistry
    {
        private readonly IEnumerable<IWFECommand> commands;

        public CommandRegistry(IEnumerable<IWFECommand> commands)
        {
            this.commands = commands;
        }

        public IWFECommand Get(string name) => commands.FirstOrDefault(a => a.GetName() == name);
    }
}
