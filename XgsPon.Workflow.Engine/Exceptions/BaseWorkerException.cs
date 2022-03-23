using System;
using System.Collections.Generic;
using System.Text;

namespace XgsPon.Workflows.Engine.Exceptions
{
    public class BaseWorkerException : Exception
    {
        public BaseWorkerException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
