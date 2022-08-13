namespace ConductorSharp.Toolkit.Models
{
    public class Configuration
    {
        public class Header
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public string ApiPath { get; set; }
        public string Namespace { get; set; }
        public string BaseUrl { get; set; }
        public string Destination { get; set; }
        public Header[] Headers { get; set; }
    }
}
