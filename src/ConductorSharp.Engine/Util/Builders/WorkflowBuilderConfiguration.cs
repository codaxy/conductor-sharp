using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util.Builders
{
    public class WorkflowBuilderConfiguration
    {
        internal WorkflowBuilderConfiguration(BuildConfiguration buildConfiguration, IEnumerable<ConfigurationProperty> configurationProperties)
        {
            BuildConfiguration = buildConfiguration;
            ConfigurationProperties = configurationProperties;
        }

        public BuildConfiguration BuildConfiguration { get; }

        public IEnumerable<ConfigurationProperty> ConfigurationProperties { get; }
    }
}
