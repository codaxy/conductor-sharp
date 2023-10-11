using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace ConductorSharp.Client.Util
{
    public class JsonDocumentConverter : JsonConverter<JsonDocument>
    {
        public override void WriteJson(JsonWriter writer, JsonDocument value, Newtonsoft.Json.JsonSerializer serializer) =>
            writer.WriteRawValue(value.RootElement.GetRawText());

        public override JsonDocument ReadJson(
            JsonReader reader,
            Type objectType,
            JsonDocument existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer
        ) => JsonDocument.Parse(JToken.FromObject(reader.Value).ToString());
    }
}
