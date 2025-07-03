using System.Net;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;

namespace ConductorSharp.Engine.IntegrationTests;

public class ConductorFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private IContainer _conductorContainer = null!;

    public async Task InitializeAsync()
    {
        var network = new NetworkBuilder().Build();

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithUsername("conductor")
            .WithPassword("conductor")
            .WithNetwork(network)
            .WithNetworkAliases("postgresdb")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilMessageIsLogged("database system is ready to accept connections", w => w.WithInterval(TimeSpan.FromSeconds(5)))
            )
            .Build();

        _conductorContainer = new ContainerBuilder()
            .WithImage("conductor:server")
            .WithEnvironment("CONFIG_PROP", "config-postgres.properties")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(
                        r => r.ForPort(8080).ForPath("/health").ForStatusCode(HttpStatusCode.OK),
                        w => w.WithInterval(TimeSpan.FromSeconds(5))
                    )
            )
            .WithNetwork(network)
            .Build();

        await _postgresContainer.StartAsync();
        await _conductorContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _conductorContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
