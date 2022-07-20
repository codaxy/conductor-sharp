using RestSharp;
using System;

namespace ConductorSharp.Client
{
    public class RestConfig
    {
        public string BaseUrl { get; set; }
        public string ApiPath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Func<RestClient> CreateClient { get; set; }
        public bool IgnoreValidationErrors { get; set; }
    }
}
