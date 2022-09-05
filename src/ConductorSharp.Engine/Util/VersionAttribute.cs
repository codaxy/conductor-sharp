using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VersionAttribute : Attribute
    {
        internal int Version { get; }

        public VersionAttribute(int version) => Version = version;
    }
}
