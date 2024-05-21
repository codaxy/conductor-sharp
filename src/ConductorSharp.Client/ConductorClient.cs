using ConductorSharp.Client.Util;

namespace ConductorSharp.Client.Generated
{
    public partial class ConductorClient
    {
        public HttpClient Client => _httpClient;

        static partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.Converters.Add(new JsonElementConverter());
            settings.Converters.Add(new JsonNodeConverter());
        }
    }
}
