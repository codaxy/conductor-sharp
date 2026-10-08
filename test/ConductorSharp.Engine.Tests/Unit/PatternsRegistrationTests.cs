using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Patterns.Extensions;
using ConductorSharp.Patterns.Services;
using ConductorSharp.Patterns.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Tests.Unit
{
    public class PatternsRegistrationTests
    {
        private static readonly ServiceProviderOptions ValidateOnBuild = new() { ValidateOnBuild = true, ValidateScopes = true };

        private static (IServiceCollection Services, IExecutionManagerBuilder Builder) CreateBuilder()
        {
            var services = new ServiceCollection();
            var builder = services
                .AddConductorSharp(baseUrl: "http://empty/empty")
                .AddExecutionManager(
                    maxConcurrentWorkers: 1,
                    sleepInterval: 1,
                    longPollInterval: 1,
                    domain: null,
                    handlerAssemblies: typeof(PatternsRegistrationTests).Assembly
                );

            return (services, builder);
        }

        [Fact]
        public void PatternsWithoutSignalWaitDoNotRequireSignalStore()
        {
            var (services, builder) = CreateBuilder();
            builder.AddConductorSharpPatterns().AddCSharpLambdaTasks();

            using var provider = services.BuildServiceProvider(ValidateOnBuild);

            Assert.DoesNotContain(services, d => d.ImplementationType == typeof(RegisterWaiter));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SignalWaitRegistersRegisterWaiterHandler(bool signalWaitFirst)
        {
            var (services, builder) = CreateBuilder();
            if (signalWaitFirst)
                builder.AddSignalWait<InMemorySignalStore>().AddConductorSharpPatterns().AddCSharpLambdaTasks();
            else
                builder.AddConductorSharpPatterns().AddCSharpLambdaTasks().AddSignalWait<InMemorySignalStore>();

            using var provider = services.BuildServiceProvider(ValidateOnBuild);

            Assert.Contains(
                services,
                d =>
                    d.ServiceType == typeof(IRequestHandler<RegisterWaiterRequest, RegisterWaiterResponse>)
                    && d.ImplementationType == typeof(RegisterWaiter)
            );
        }
    }
}
