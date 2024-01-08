using ConductorSharp.Client.Generated;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Client.Service
{
    public class MetadataService
    {
        private readonly ConductorClient _conductorClient;

        public MetadataService(ConductorClient client) => _conductorClient = client;
    }
}
