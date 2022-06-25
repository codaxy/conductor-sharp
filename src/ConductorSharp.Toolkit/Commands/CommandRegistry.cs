namespace ConductorSharp.Toolkit.Commands
{
    public class CommandRegistry
    {
        private readonly IEnumerable<Command> commands;

        public CommandRegistry(IEnumerable<Command> commands)
        {
            this.commands = commands;
        }

        public Command Get(string name) => commands.FirstOrDefault(a => a.GetName() == name);
    }
}
