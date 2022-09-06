using CommandLine;

namespace ConductorSharp.Toolkit.Models
{
    public class ToolkitOptions
    {
        [Option('f', "file", HelpText = "Configuration file", Default = "conductorsharp.yaml")]
        public string ConfigurationFilePath { get; set; }

        [Option('n', "name", HelpText = "Specifies names of the tasks and workflows to scaffold (workflow and task filter)")]
        public IEnumerable<string> NameFilters { get; set; }

        [Option('a', "app", HelpText = "Specifies owner apps of the workflows to scaffold (workflow filter)")]
        public IEnumerable<string> OwnerAppFilters { get; set; }

        [Option('e', "email", HelpText = "Specifies owner emails of the workflows to scaffold (workflow filter)")]
        public IEnumerable<string> OwnerEmailFilters { get; set; }
    }
}
