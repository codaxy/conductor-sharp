using System;

namespace ConductorSharp.Patterns.Exceptions
{
    public class LambdaTasksNotEnabledException : Exception
    {
        public LambdaTasksNotEnabledException() : base("Call AddCSharpLambdaTasks() after AddExecutionManager() method to enable the feature") { }
    }
}
