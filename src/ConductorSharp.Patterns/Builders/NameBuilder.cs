using System;
using System.Collections.Generic;
using System.Linq;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Tasks;
using ConductorSharp.Patterns.Workflows;

namespace ConductorSharp.Patterns.Builders
{
    internal class NameBuilder(IEnumerable<ConfigurationProperty> configurationProperties) : DefaultNameBuilder
    {
        private readonly IEnumerable<ConfigurationProperty> _configurationProperties = configurationProperties;

        public override string Build(Type typeToName)
        {
            if (typeToName == typeof(CSharpLambdaTask))
                return $"{GetLambdaTaskPrefix()}{base.Build(typeToName)}";

            if (typeToName == typeof(RegisterWaiter) || typeToName == typeof(SignalWait))
                return $"{GetSignalWaitPrefix()}{base.Build(typeToName)}";

            return base.Build(typeToName);
        }

        private string GetLambdaTaskPrefix()
        {
            var prefix = (string?)
                _configurationProperties.FirstOrDefault(prop => prop.Key == CSharpLambdaTask.LambdaTaskNameConfigurationProperty)?.Value;
            return prefix == null ? string.Empty : $"{prefix}.";
        }

        private string GetSignalWaitPrefix()
        {
            var prefix = (string?)_configurationProperties.FirstOrDefault(prop => prop.Key == Constants.SignalPrefixConfigurationProperty)?.Value;
            return prefix == null ? string.Empty : $"{prefix}_";
        }
    }
}
