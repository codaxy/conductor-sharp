﻿namespace ConductorSharp.Toolkit.Models
{
    public class Configuration
    {
        public string ApiPath { get; set; }
        public string Namespace { get; set; }
        public string BaseUrl { get; set; }
        public bool Dryrun { get; set; }
        public string Destination { get; set; }
    }
}
