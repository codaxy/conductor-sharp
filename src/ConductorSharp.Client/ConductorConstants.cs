using ConductorSharp.Client.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Client
{
    public static class ConductorConstants
    {
        public static NamingStrategy IoNamingStrategy { get; } = new SnakeCaseNamingStrategy();

        public static JsonSerializer IoJsonSerializer
        {
            get => JsonSerializer.Create(IoJsonSerializerSettings);
        }

        public static JsonSerializerSettings IoJsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = IoNamingStrategy },
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = new List<JsonConverter>() { new JsonNodeConverter(), new JsonElementConverter(), new StringEnumConverter() }
                };
            }
        }

        public static JsonSerializer DefinitionsSerializer { get; } = new() { NullValueHandling = NullValueHandling.Ignore };
    }
}
