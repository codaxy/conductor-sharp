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
            Console.WriteLine(options.ConfiugrationFilePath);

            if (!File.Exists(options.ConfiugrationFilePath))
            {
                Console.Error.WriteLine($"Configuration file {options.ConfiugrationFilePath} does not exists");
                return;
            }

            var config = await ParseConfigurationFile(options.ConfiugrationFilePath);
            var container = BuildContainer(config);
            var commandRegistry = container.Resolve<CommandRegistry>();
            // Currently only scaffolding is supported
            var command = commandRegistry.Get("scaffold");
            await command.Execute(config);
        }

        private static async Task<Configuration> ParseConfigurationFile(string configFilePath) =>
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize<Configuration>(await File.ReadAllTextAsync(configFilePath));

        private static IContainer BuildContainer(Configuration input)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();

            var builder = new ContainerBuilder();

            serviceCollection.Configure<ScaffoldingConfig>(config =>
            {
                config.ApiUrl = input.Api;
                config.BaseUrl = input.Host;
                config.BaseNamespace = input.Namespace;
                config.Dryrun = input.Dryrun;
                config.Destination = input.Destination;
            });

            builder.Populate(serviceCollection);
            builder.AddWorkflowEngine(input.Host, input.Api);
            builder.RegisterModule(new ToolkitModule());

            return builder.Build();
        }

        private static Configuration ParseInput(string[] args)
        {
            var action = args[0];
            var inputParameters = args.Skip(1).Select(a => new KeyValuePair<string, string>(a.Split("=")[0], a.Split("=")[1])).ToList();

            return new Configuration
            {
                Api = inputParameters.Where(a => a.Key == "path").Select(a => a.Value).FirstOrDefault(),
                Namespace = inputParameters.Where(a => a.Key == "namespace").Select(a => a.Value).FirstOrDefault(),
                Host = inputParameters.Where(a => a.Key == "host").Select(a => a.Value).FirstOrDefault(),
                Dryrun = inputParameters.Where(a => a.Key == "dryrun").Select(a => bool.Parse(a.Value)).FirstOrDefault(),
                Destination = inputParameters.Where(a => a.Key == "destination").Select(a => a.Value).FirstOrDefault()
            };
        }
    }
}
