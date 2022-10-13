using System;

namespace ConductorSharp.Engine.Util
{
    public class CSharpLambda
    {
        public CSharpLambda(string lambdaIdentifier, Delegate handler, Type inputType)
        {
            LambdaIdentifier = lambdaIdentifier;
            Handler = handler;
            InputType = inputType;
        }

        public string LambdaIdentifier { get; }
        public Delegate Handler { get; }
        public Type InputType { get; }
    }
}
