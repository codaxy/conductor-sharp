using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Toolkit.Commands;
using ConductorSharp.Toolkit.Models;
using CommandLine;
using CommandLine.Text;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace ConductorSharp.Toolkit
{
    class Program
    {
        public async static Task Main(string[] args)
        {
            var parseResult = new Parser(opts => opts.HelpWriter = null).ParseArguments<ToolkitOptions>(args);
            var withParsed = await parseResult.WithParsedAsync(RunToolkit);
            withParsed.WithNotParsed(err =>
            {
                var versionText = new HeadingInfo("conductorsharp", GetVersionString());
                var writer = err.IsHelp() || err.IsVersion() ? Console.Out : Console.Error;
                string textToWrite;
                if (err.IsVersion())
                    textToWrite = versionText;
                else
                {
                    textToWrite = HelpText.AutoBuild(
                        parseResult,
                        help =>
                        {
                            help.AddPreOptionsLine("Usage: dotnet conductorsharp [options]");
                            help.Heading = versionText;
                            return help;
                        }
                    );
                }

                writer.WriteLine(textToWrite);
            });
        }

        private static string GetVersionString()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"version {version.Major}.{version.Minor}.{version.Build}";
        }

        private static async Task RunToolkit(ToolkitOptions options)
        {
            try
            {
                if (!File.Exists(options.ConfigurationFilePath))
                {
                    Console.Error.WriteLine($"Configuration file {options.ConfigurationFilePath} does not exists");
                    return;
                }

                var config = await ParseConfigurationFile(options.ConfigurationFilePath);
                var container = BuildContainer(config);
                var commandRegistry = container.Resolve<CommandRegistry>();
                // Currently only scaffolding is supported
                var command = commandRegistry.Get("scaffold");
                await command.Execute(config);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception occured with message: {ex.Message}");
            }
        }

        private static async Task<Configuration> ParseConfigurationFile(string configFilePath) =>
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<Configuration>(await File.ReadAllTextAsync(configFilePath));

        private static IContainer BuildContainer(Configuration config)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();

            var builder = new ContainerBuilder();

            serviceCollection.Configure<ScaffoldingConfig>(scaffoldingConfig =>
            {
                scaffoldingConfig.ApiUrl = config.ApiPath;
                scaffoldingConfig.BaseUrl = config.BaseUrl;
                scaffoldingConfig.BaseNamespace = config.Namespace;
                scaffoldingConfig.Destination = config.Destination;
            });

            builder.Populate(serviceCollection);
            builder.AddWorkflowEngine(
                config.BaseUrl,
                config.ApiPath,
                createClient: () =>
                {
                    var client = new RestClient();
                    client.UseNewtonsoftJson();
                    client.AddDefaultHeaders(config.Headers);
                    return client;
                }
            );
            builder.RegisterModule(new ToolkitModule());

            return builder.Build();
        }
    }
}
