using System;

namespace ConductorSharp.Patterns.Exceptions
{
    public class NoLambdaException : Exception
    {
        public NoLambdaException(string lambdaName) : base($"ConductorSharp: Lambda with corresponding identifier\"{lambdaName}\" ") { }
    }
}
