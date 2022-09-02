using CommandLine;

namespace ConductorSharp.Toolkit.Models
{
    public class ToolkitOptions
    {
        [Option('f', "file", HelpText = "Configuration file", Default = "conductorsharp.yaml")]
        public string ConfigurationFilePath { get; set; }

        [Option('n', "name", HelpText = "Specifies names of the tasks and workflows to scaffold")]
        public IEnumerable<string> Names { get; set; }
    }
}
