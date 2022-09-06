using CommandLine;

namespace ConductorSharp.Toolkit.Models
{
    public class ToolkitOptions
    {
        [Option('f', "file", HelpText = "Configuration file", Default = "conductorsharp.yaml")]
        public string ConfigurationFilePath { get; set; }

        [Option('n', "name", HelpText = "Specifies names of the tasks and workflows to scaffold")]
        public IEnumerable<string> NameFilters { get; set; }

        [Option('a', "app", HelpText = "Specifies owner apps of the tasks and workflows to scaffold")]
        public IEnumerable<string> OwnerAppFilters { get; set; }

        [Option('e', "email", HelpText = "Specifies owner emails of the tasks and workflows to scaffold")]
        public IEnumerable<string> OwnerEmailFilters { get; set; }

        [Option("no-tasks", Default = false, SetName = "no-tasks")]
        public bool IgnoreTasks { get; set; }

        [Option("no-workflows", Default = false, SetName = "no-workflows")]
        public bool IgnoreWorkflows { get; set; }
    }
}
