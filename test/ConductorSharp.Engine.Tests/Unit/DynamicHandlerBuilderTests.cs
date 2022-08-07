using System.Reflection;
using System.Security.Cryptography;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class DynamicHandlerBuilderTests
    {
        public class Input : IRequest<Output>
        {
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
            var methodInfo = obj.GetType().GetMethod(nameof(ITaskRequestHandler<Input, Output>.Handle));
            var output = await (Task<Output>)
                methodInfo.Invoke(
                    obj,
                    new object[]
                    {
                        new Input { Number = 0 },
                        new CancellationToken()
                    }
                );

            Assert.Equal(1, output.Number);

            var originalNameAttribute = obj.GetType().GetCustomAttribute<OriginalNameAttribute>();
            Assert.NotNull(originalNameAttribute);
            Assert.Equal(expectedTaskName, originalNameAttribute.OriginalName);
        }
    }
}
