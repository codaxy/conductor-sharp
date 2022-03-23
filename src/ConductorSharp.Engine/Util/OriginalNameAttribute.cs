using System;

namespace ConductorSharp.Engine.Util;

[AttributeUsage(AttributeTargets.Class)]
public class OriginalNameAttribute : Attribute
{
    public string OriginalName { get; }
    public OriginalNameAttribute(string originalName) => OriginalName = originalName;
}
