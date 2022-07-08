namespace ConductorSharp.Toolkit.Commands
{
    public class CommandRegistry
    {
        private readonly IEnumerable<Command> _commands;

        public CommandRegistry(IEnumerable<Command> commands)
        {
            _commands = commands;
        }

        public Command Get(string name) => _commands.FirstOrDefault(a => a.GetName() == name);
    }
}
