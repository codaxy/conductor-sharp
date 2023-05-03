using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Client
{
    public static class ConductorConstants
    {
        public static string SimpleTask => "SIMPLE";

        public static NamingStrategy IoNamingStrategy { get; } = new SnakeCaseNamingStrategy();

        public static JsonSerializer IoJsonSerializer { get; } =
            new()
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = IoNamingStrategy },
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.Auto
            };
        public static JsonSerializer DefinitionsSerializer { get; } = new() { NullValueHandling = NullValueHandling.Ignore };
    }
}
