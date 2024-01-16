using System;

namespace ConductorSharp.Engine.Builders.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class VersionAttribute(int version) : Attribute
{
    internal int Version { get; } = version;
}