using System;

namespace ConductorSharp.Engine.Util
{
    internal class CSharpLambdaHandler(string lambdaIdentifier, Type taskInputType, Delegate handler)
    {
        public string LambdaIdentifier { get; } = lambdaIdentifier;
        public Type TaskInputType { get; } = taskInputType;
        public Delegate Handler { get; } = handler;
    }
}
