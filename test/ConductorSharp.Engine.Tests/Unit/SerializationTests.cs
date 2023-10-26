using ConductorSharp.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    }
}
