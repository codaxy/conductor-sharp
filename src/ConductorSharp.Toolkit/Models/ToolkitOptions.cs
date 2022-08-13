using CommandLine;

namespace ConductorSharp.Toolkit.Models
{
    public class ToolkitOptions
    {
        [Option('f', "file", HelpText = "Configuration file", Default = "conductorsharp.yaml")]
        public string ConfiugrationFilePath { get; set; }
    }
}
