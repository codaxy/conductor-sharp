using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ConductorSharp.Client.Util
{
    public class JsonElementConverter : JsonConverter<JsonElement>
    {
        public override void WriteJson(JsonWriter writer, JsonElement value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, JToken.Parse(value.GetRawText()));
        }

        public override JsonElement ReadJson(
            JsonReader reader,
            Type objectType,
            JsonElement existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        ) => System.Text.Json.JsonSerializer.Deserialize<JsonElement>(JToken.Load(reader).ToString(Formatting.None));
    }
}
