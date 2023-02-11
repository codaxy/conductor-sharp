using System;

namespace ConductorSharp.Engine.Interface
{
    public interface ITaskNameBuilder
    {
        string Build(Type taskType);
    }
}
