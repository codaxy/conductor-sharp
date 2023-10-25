using ConductorSharp.Client.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Client
{
    // TODO: Remove this class
    public static class ConductorConstants
    {
        public static string SimpleTask => "SIMPLE";

        public static NamingStrategy IoNamingStrategy { get; } = new SnakeCaseNamingStrategy();

        public static JsonSerializer DefinitionsSerializer { get; } = new() { NullValueHandling = NullValueHandling.Ignore };
    }
}
