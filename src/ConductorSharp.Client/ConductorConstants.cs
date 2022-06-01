using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Client
{
    public static class ConductorConstants
    {
        //Test
        public static string SimpleTask { get; } = "SIMPLE";
        public static string SubworkflowTask { get; } = "SUB_WORKFLOW";
        public static JsonSerializer IoJsonSerializer { get; } =
            new()
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.Auto
            };
    }
}
