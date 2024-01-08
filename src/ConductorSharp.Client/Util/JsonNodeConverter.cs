using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace ConductorSharp.Client.Util
{
    public class JsonNodeConverter : JsonConverter<JsonNode>
    {
        public override void WriteJson(JsonWriter writer, JsonNode? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }

            serializer.Serialize(writer, JToken.Parse(value.ToJsonString()));
        }

        public override JsonNode? ReadJson(
            JsonReader reader,
            Type objectType,
            JsonNode? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        ) => JsonNode.Parse(JToken.Load(reader).ToString(Formatting.None));
    }
}
