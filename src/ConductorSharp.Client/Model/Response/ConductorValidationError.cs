using Newtonsoft.Json;

namespace ConductorSharp.Client.Model.Response;

public class ConductorValidationError
{
    [JsonProperty("path")]
    public string Path { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
}
