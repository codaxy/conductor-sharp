using ConductorSharp.Client.Util;
using ConductorSharp.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Engine.Util
{
    public static class Serializers
    {
        static Serializers()
        {
            IOSerializer = new JsonSerializer
            {
                ContractResolver = new IOContractResolver { NamingStrategy = ConductorConstants.IoNamingStrategy },
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.Auto
            };

            IOSerializer.Converters.Add(new JsonValueConverter());
            IOSerializer.Converters.Add(new JsonDocumentConverter());
        }

        public static JsonSerializer IOSerializer { get; }
    }
}
