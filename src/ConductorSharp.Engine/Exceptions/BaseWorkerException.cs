using System;

namespace ConductorSharp.Engine.Exceptions;

public class BaseWorkerException : Exception
{
    public BaseWorkerException(string message, Exception innerException)
        : base(message, innerException) { }
}
