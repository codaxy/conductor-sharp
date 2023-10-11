using Newtonsoft.Json;
using System;
using System.Text.Json.Nodes;

namespace ConductorSharp.Client.Util
{
    public class JsonValueConverter : JsonConverter<JsonValue>
    {
        public override void WriteJson(JsonWriter writer, JsonValue value, Newtonsoft.Json.JsonSerializer serializer) =>
            writer.WriteValue(value.ToString());

        public override JsonValue ReadJson(
            JsonReader reader,
            Type objectType,
            JsonValue existingValue,
            bool hasExistingValue,
            Newtonsoft.Json.JsonSerializer serializer
        ) => JsonValue.Create(reader.Value);
    }
}
