using ConductorSharp.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class SerializationTests
    {
        public class User
        {
            public JsonDocument JDocIdentity { get; set; }
            public JObject JobjIdentity { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void HandlesJsonDocumentDeserialization()
        {
            var user = new User { Id = 1 };
            var anon = new { Name = "TestName", Surname = "TestSurname" };

            user.JDocIdentity = System.Text.Json.JsonSerializer.SerializeToDocument(anon);
            user.JobjIdentity = JObject.FromObject(anon);

            var job = JObject.FromObject(user, ConductorConstants.IoJsonSerializer);
            var userDeserialized = job.ToObject<User>(ConductorConstants.IoJsonSerializer);

            Assert.NotNull(userDeserialized);
            Assert.Equal(userDeserialized.JDocIdentity.RootElement.ToString(), userDeserialized.JobjIdentity.ToString(Formatting.None));
        }
    }
}
