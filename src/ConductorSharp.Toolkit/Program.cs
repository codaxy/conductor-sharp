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
        public const string Version = "2.0.0-alpha1";

        public async static Task Main(string[] args)
        {
            var parseResult = new Parser(opts => opts.HelpWriter = null).ParseArguments<ToolkitOptions>(args);
            var withParsed = await parseResult.WithParsedAsync(RunToolkit);
            withParsed.WithNotParsed(err =>
            {
                var versionText = new HeadingInfo("conductorsharp", Version);
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
                            help.Copyright = new CopyrightInfo("Codaxy", 2022);
                            help.AddPreOptionsLine("Usage: dotnet conductorsharp [options]");
                            help.Heading = versionText;
                            return help;
                        }
                    );
                }

                writer.WriteLine(textToWrite);
            });
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

                var config = ParseConfigurationFile(options.ConfigurationFilePath);
                if (!ValidateConfiguration(config))
                    return;

                var container = BuildContainer(config, options);
                var commandRegistry = container.Resolve<CommandRegistry>();
                // Currently only scaffolding is supported
                var command = commandRegistry.Get("scaffold");
                await command.Execute(config);
            }
            // TODO: Improve error handling
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception occured with message: {ex.Message}");
                Console.Error.WriteLine("Stack trace:");
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        private static Configuration ParseConfigurationFile(string configFilePath) =>
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<Configuration>(File.ReadAllText(configFilePath));

        private static bool ValidateConfiguration(Configuration config)
        {
            bool validConfiguration = true;
            if (string.IsNullOrEmpty(config.BaseUrl))
            {
                Console.Error.WriteLine("baseUrl property missing in configuration");
                validConfiguration = false;
            }
            if (string.IsNullOrEmpty(config.Namespace))
            {
                Console.Error.WriteLine("namespace property missing in configuration");
                validConfiguration = false;
            }
            if (string.IsNullOrEmpty(config.Destination))
            {
                Console.Error.WriteLine("destination property missing in configuration");
                validConfiguration = false;
            }

            return validConfiguration;
        }

        private static IContainer BuildContainer(Configuration config, ToolkitOptions options)
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
                scaffoldingConfig.NameFilters = options.NameFilters.ToArray();
                scaffoldingConfig.OwnerAppFilters = options.OwnerAppFilters.ToArray();
                scaffoldingConfig.OwnerEmailFilters = options.OwnerEmailFilters.ToArray();
                scaffoldingConfig.IgnoreTasks = options.IgnoreTasks;
                scaffoldingConfig.IgnoreWorkflows = options.IgnoreWorkflows;
                scaffoldingConfig.DryRun = options.DryRun;
            });

            builder.Populate(serviceCollection);
            builder.AddConductorSharp(
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
