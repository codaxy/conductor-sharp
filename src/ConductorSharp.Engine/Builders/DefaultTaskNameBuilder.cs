using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using System;

namespace ConductorSharp.Engine.Builders
{
    public class DefaultTaskNameBuilder : ITaskNameBuilder
    {
        public string Build(Type taskType) => NamingUtil.DetermineRegistrationName(taskType);
    }
}
