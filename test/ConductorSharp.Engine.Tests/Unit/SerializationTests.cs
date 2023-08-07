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
            public JsonValue JDocIdentity { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void HandlesJsonValueDeserialization()
        {
            var user = new User { Id = 1 };
            var anon = "TEST";

            user.JDocIdentity = JsonValue.Create(anon);

            var job = JObject.FromObject(user, ConductorConstants.IoJsonSerializer);
            var userDeserialized = job.ToObject<User>(ConductorConstants.IoJsonSerializer);

            Assert.NotNull(userDeserialized);
            Assert.Equal("TEST", userDeserialized.JDocIdentity.GetValue<string>());
        }
    }
}
