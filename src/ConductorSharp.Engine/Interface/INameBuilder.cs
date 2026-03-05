using System;

namespace ConductorSharp.Engine.Interface
{
    public interface INameBuilder
    {
        string Build(Type typeToName);
    }
}
