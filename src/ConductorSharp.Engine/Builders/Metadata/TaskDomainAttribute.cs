using System;

namespace ConductorSharp.Engine.Builders.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class TaskDomainAttribute(string domain) : Attribute
{
    internal string Domain { get; set; } = domain;
}
