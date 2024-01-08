using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConductorSharp.Patterns.Builders
{
    internal class TaskNameBuilder(IEnumerable<ConfigurationProperty> configurationProperties) : DefaultTaskNameBuilder
    {
        private readonly IEnumerable<ConfigurationProperty> _configurationProperties = configurationProperties;

        public override string Build(Type taskType) =>
            taskType == typeof(CSharpLambdaTask) ? $"{GetLambdaTaskPrefix()}{base.Build(taskType)}" : base.Build(taskType);

        private string GetLambdaTaskPrefix()
        {
            var prefix = (string)_configurationProperties.First(prop => prop.Key == CSharpLambdaTask.LambdaTaskNameConfigurationProperty).Value;
            return MakeTaskNamePrefix(prefix);
        }

        public static string MakeTaskNamePrefix(string prefix) => prefix == null ? string.Empty : $"{prefix}.";
    }
}
