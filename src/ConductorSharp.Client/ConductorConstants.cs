using ConductorSharp.Client.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Client
{
    public static class ConductorConstants
    {
        public static string SimpleTask => "SIMPLE";

        public static NamingStrategy IoNamingStrategy { get; } = new SnakeCaseNamingStrategy();

        public static JsonSerializerSettings ConductorClientJsonSerializerSettings
        {
            get
            {
                JsonSerializerSettings settings =
                    new()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                    };

                settings.Converters.Add(new JsonDocumentConverter());
                return settings;
            }
        }
        public static JsonSerializer IoJsonSerializer
        {
            get
            {
                var serializer = new JsonSerializer()
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = IoNamingStrategy },
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                serializer.Converters.Add(new JsonDocumentConverter());
                return serializer;
            }
        }

        public static JsonSerializer DefinitionsSerializer { get; } = new() { NullValueHandling = NullValueHandling.Ignore };
    }
}
