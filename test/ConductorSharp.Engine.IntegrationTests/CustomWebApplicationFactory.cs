using System.Diagnostics;
using System.Net;
using ConductorSharp.ApiEnabled.Workflows;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Util;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace ConductorSharp.Engine.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
                    .UntilMessageIsLogged("SystemTaskWorkerCoordinator initialized with 5 async tasks", w => w.WithInterval(TimeSpan.FromSeconds(5)))
            )
            .WithNetwork(network)
            .Build();

        await _postgresContainer.StartAsync();
        await _conductorContainer.StartAsync();

        var metadataService = Services.GetRequiredService<IMetadataService>();
        var timeout = TimeSpan.FromSeconds(10);
        var deploymentStopwatch = Stopwatch.StartNew();
        var wfDeployed = false;

        while (deploymentStopwatch.Elapsed < timeout)
        {
            var wfs = await metadataService.ListWorkflowsAsync();
            wfDeployed = wfs.Any(wf => wf.Name == NamingUtil.NameOf<TestWorkflow.Workflow>());

            if (wfDeployed)
                break;

            await Task.Delay(1000);
        }

        if (!wfDeployed)
            throw new TimeoutException("Timeout during workflow deployment");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>()
                {
                    { "Conductor:BaseUrl", $"http://{_conductorContainer.Hostname}:{_conductorContainer.GetMappedPublicPort(8080)}" }
                }!
            )
            .Build();

        builder.UseConfiguration(config);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _conductorContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
