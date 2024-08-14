using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Util
{
    internal class ApiPathOverrideHttpHandler(string apiPath) : DelegatingHandler
    {
        private readonly Regex _pathRegex = new("api");

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            request.RequestUri = OverridePath(uri);
            return base.SendAsync(request, cancellationToken);
        }

        private Uri OverridePath(Uri uri)
        {
            var overridenPathAndQuery = _pathRegex.Replace(uri.PathAndQuery, apiPath, 1);
            return new Uri(new Uri(uri.Scheme + "://" + uri.Authority), overridenPathAndQuery);
        }
    }
}
