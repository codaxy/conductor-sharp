using System;

namespace ConductorSharp.Engine.Util
{
    internal class CSharpLambdaHandler
    {
        public string LambdaIdentifier { get; }
        public Type TaskInputType { get; }
        public Delegate Handler { get; }

        public CSharpLambdaHandler(string lambdaIdentifier, Type taskInputType, Delegate handler)
        {
            LambdaIdentifier = lambdaIdentifier;
            TaskInputType = taskInputType;
            Handler = handler;
        }
    }
}
