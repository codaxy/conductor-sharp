using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public interface IConductorClient
    {
        Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource);
        Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers);
        Task<T> ExecuteRequestAsync<T>(Uri relativeUrl, HttpMethod method);
        Task ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource);
        Task ExecuteRequestAsync(Uri relativeUrlrelativeUrl, HttpMethod method);
        Task<string> ExecuteRequestAsync(Uri relativeUrl, HttpMethod method, object resource, Dictionary<string, string> headers);
    }
}
