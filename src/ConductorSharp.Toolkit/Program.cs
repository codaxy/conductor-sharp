﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Toolkit.Commands;
using ConductorSharp.Toolkit.Models;

namespace ConductorSharp.Toolkit
{
    class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                var action = args[0];

                var input = ParseInput(args);

                var container = BuildContainer(input);

                var commandRegistry = container.Resolve<CommandRegistry>();

                var command = commandRegistry.Get(action);

                await command.Execute(input);
            }
            catch (Exception exc)
            {
                PrintHelp();
            }
        }

        private static void PrintHelp() => Console.WriteLine("PLACEHOLDER HELP");

        private static IContainer BuildContainer(CommandInput input)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();

            var builder = new ContainerBuilder();

            serviceCollection.Configure<ScaffoldingConfig>(
                config =>
                {
                    config.ApiUrl = input.Api;
                    config.BaseUrl = input.Host;
                    config.BaseNamespace = input.Namespace;
                    config.Dryrun = input.Dryrun;
                    config.Destination = input.Destination;
                }
            );

            builder.Populate(serviceCollection);
            builder.AddWorkflowEngine(input.Host, input.Api);
            builder.RegisterModule(new ToolkitModule());

            return builder.Build();
        }

        private static CommandInput ParseInput(string[] args)
        {
            var action = args[0];
            var inputParameters = args.Skip(1).Select(a => new KeyValuePair<string, string>(a.Split("=")[0], a.Split("=")[1])).ToList();

            return new CommandInput
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
