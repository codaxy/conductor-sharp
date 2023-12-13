using ConductorSharp.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class SerializationTests
    {
        public class User
        {
            public JsonNode JDocString { get; set; }
            public JsonNode JDocId { get; set; }
            public JsonNode JDocArray { get; set; }
            public JsonNode JDocBool { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void HandlesJsonValueDeserialization()
        {
            var user = new User
            {
                Id = 1,
                JDocString = JsonValue.Create("TEST"),
                JDocId = JsonValue.Create(2),
                JDocArray = JsonValue.Create(new int[] { 1, 2, 3, 4 }),
                JDocBool = JsonValue.Create(false)
            };

            var job = JObject.FromObject(user, ConductorConstants.IoJsonSerializer);
            var userDeserialized = job.ToObject<User>(ConductorConstants.IoJsonSerializer);

            Assert.NotNull(userDeserialized);
            Assert.Equal(user.JDocString.ToString(), userDeserialized.JDocString.ToString());
            Assert.Equal(user.JDocBool.ToString(), userDeserialized.JDocBool.ToString());
            Assert.Equal(user.JDocArray.ToString(), userDeserialized.JDocArray.ToString());
            Assert.Equal(user.JDocId.ToString(), userDeserialized.JDocId.ToString());
        }

        public class Characteristic
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ValueType { get; set; }
            public JsonNode Value { get; set; }

            [JsonPropertyName("@baseType")]
            [JsonProperty("@baseType")]
            public string @BaseType { get; set; }

            [JsonPropertyName("@schemaLocation")]
            [JsonProperty("@schemaLocation")]
            public string @SchemaLocation { get; set; }

            [JsonPropertyName("@type")]
            [JsonProperty("@type")]
            public string @Type { get; set; }
        }

        public class Service
        {
            [JsonPropertyName("service_characteristic")]
            [JsonProperty("service_characteristic")]
            public Characteristic[] ServiceCharacteristic { get; set; }
        }

        [Fact]
        public void ToObjectDoesNotThrowWhenTypesArePresentInJObject()
        {
            var payload =
                "{\"service_characteristic\": [{\"name\": \"preflight-checks\",\"value_type\": \"object\",\"value\": {\"CheckVersion\": {\"enabled\": \"true\"}}}]}";

            var jobject = JObject.Parse(payload);
            Assert.NotNull(jobject);

            var service = jobject.ToObject<Service>(ConductorConstants.IoJsonSerializer);
            Assert.NotNull(service);

            var csoJson = JsonConvert.SerializeObject(
                service,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                    TypeNameHandling = TypeNameHandling.Auto
                }
            );

            Assert.NotNull(csoJson);
            Assert.Contains("\"$type\":\"System.Text.Json.Nodes.JsonObject, System.Text.Json\"", csoJson);

            var jobject2 = JObject.Parse(csoJson);
            var data2 = jobject2.ToObject<Service>(ConductorConstants.IoJsonSerializer);
            Assert.NotNull(data2);
        }
    }
}
