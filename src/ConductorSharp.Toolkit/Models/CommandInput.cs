namespace ConductorSharp.Toolkit.Models
{
    public class CommandInput
    {
        public string Action { get; set; }
        public string Api { get; set; }
        public string Namespace { get; set; }
        public string Host { get; set; }
        public bool Dryrun { get; set; }
        public string Destination { get; set; }
        public string YmlDestination { get; set; }
        public string Source { get; set; }
    }
}
