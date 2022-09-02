namespace ConductorSharp.Toolkit
{
    public class ScaffoldingConfig
    {
        public string BaseNamespace { get; set; }
        public string ApiUrl { get; set; }
        public string BaseUrl { get; set; }
        public bool Dryrun { get; set; }
        public string Destination { get; set; }
        public string[] Names { get; set; }
    }
}
