using System;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Builders
{
    public class DefaultNameBuilder : INameBuilder
    {
        public virtual string Build(Type typeToName) => NamingUtil.DetermineRegistrationName(typeToName);
    }
}
