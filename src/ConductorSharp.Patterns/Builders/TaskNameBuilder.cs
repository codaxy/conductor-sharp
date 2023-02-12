using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Patterns.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorSharp.Patterns.Builders
{
    internal class TaskNameBuilder : DefaultTaskNameBuilder
    {
        private readonly IEnumerable<ConfigurationProperty> _configurationProperties;

        public TaskNameBuilder(IEnumerable<ConfigurationProperty> configurationProperties) => _configurationProperties = configurationProperties;

        public override string Build(Type taskType) =>
            taskType == typeof(CSharpLambdaTask) ? $"{GetLambdaTaskPrefix()}.{base.Build(taskType)}" : base.Build(taskType);

        private string GetLambdaTaskPrefix() =>
            (string)_configurationProperties.First(prop => prop.Key == CSharpLambdaTask.LambdaTaskNameConfigurationProperty).Value;
    }
}
