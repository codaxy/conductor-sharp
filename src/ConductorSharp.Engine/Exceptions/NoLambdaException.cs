using System;

namespace ConductorSharp.Engine.Exceptions
{
    public class NoLambdaException : Exception
    {
        public NoLambdaException(string lambdaName) : base($"ConductorSharp: Lambda with corresponding identifier\"{lambdaName}\" ") { }
    }
}
