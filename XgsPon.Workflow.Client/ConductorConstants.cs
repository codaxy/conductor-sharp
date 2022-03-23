using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace XgsPon.Workflows.Client
{
    public static class ConductorConstants
    {
        public static string SimpleTask { get; } = "SIMPLE";
        public static string SubworkflowTask { get; } = "SUB_WORKFLOW";
        public static JsonSerializer IoJsonSerializer { get; } =
            new()
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.Auto
            };
    }
}
