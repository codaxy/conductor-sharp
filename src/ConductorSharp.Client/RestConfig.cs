using RestSharp;
using System;
using System.Collections.Generic;

namespace ConductorSharp.Client
{
    public class RestConfig
    {
        public string BaseUrl { get; set; }
        public string ApiPath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> DefaultHeaders { get; set; }
        public bool IgnoreValidationErrors { get; set; }
    }
}
