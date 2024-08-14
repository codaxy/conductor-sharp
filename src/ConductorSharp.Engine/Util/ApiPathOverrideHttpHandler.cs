using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Util
{
    internal class ApiPathOverrideHttpHandler(string apiPath) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var overridenUri = new Uri(uri.ToString().Replace("api", apiPath));
            request.RequestUri = overridenUri;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
