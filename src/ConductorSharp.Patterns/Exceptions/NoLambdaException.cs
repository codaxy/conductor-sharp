using System;

namespace ConductorSharp.Patterns.Exceptions
{
    public class NoLambdaException(string lambdaName) : Exception($"ConductorSharp: Lambda with corresponding identifier\"{lambdaName}\" ") { }
}
