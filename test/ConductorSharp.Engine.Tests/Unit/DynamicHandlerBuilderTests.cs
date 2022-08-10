using System.Reflection;
using System.Security.Cryptography;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class DynamicHandlerBuilderTests
    {
        private const string PropName = "MyVar";

        public class Input : IRequest<Output>
        {
            [JsonProperty(PropName)]
            public int Number { get; set; }
        }

        public class Output
        {
            public int Number { get; set; }
        }

        [Fact]
        public async Task BuildsCorrectType()
        {
            const string expectedTaskName = "TestTaskName";
            var dynamicHandlerBuilder = DynamicHandlerBuilder.DefaultBuilder;

            dynamicHandlerBuilder.AddDynamicHandler((Input input) => new Output { Number = input.Number + 1 }, expectedTaskName);

            Assert.Single(dynamicHandlerBuilder.Handlers);
            var handler = dynamicHandlerBuilder.Handlers[0];
            var obj = handler.CreateInstance();

            var originalNameAttribute = obj.GetType().GetCustomAttribute<OriginalNameAttribute>();
            Assert.NotNull(originalNameAttribute);
            Assert.Equal(expectedTaskName, originalNameAttribute.OriginalName);

            var proxyInputType = obj.GetType().BaseType.GetGenericArguments()[0];
            var inputObj = Activator.CreateInstance(proxyInputType);
            var inputProp = proxyInputType.GetProperty(nameof(Input.Number));
            var jsonPropAttr = inputProp.GetCustomAttribute<JsonPropertyAttribute>();
            Assert.NotNull(jsonPropAttr);
            Assert.Equal(PropName, jsonPropAttr.PropertyName);
            inputProp.SetValue(inputObj, 1);

            var methodInfo = obj.GetType().GetMethod(nameof(ITaskRequestHandler<Input, Output>.Handle));
            var output = await (Task<Output>)methodInfo.Invoke(obj, new object[] { inputObj, new CancellationToken() });

            Assert.Equal(2, output.Number);
        }
    }
}
