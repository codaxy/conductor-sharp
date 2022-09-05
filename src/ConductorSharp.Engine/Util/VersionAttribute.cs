using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VersionAttribute : Attribute
    {
        public int Version { get; }

        public VersionAttribute(int version) => Version = version;
    }
}
