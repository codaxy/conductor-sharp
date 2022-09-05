using ConductorSharp.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Interface
{
    public interface ITaskOptionsBuilder
    {
        /// <summary>
        /// Enabling this means the workflow continues even if the task fails. The status of the task is reflected as COMPLETED_WITH_ERRORS
        /// </summary>
        /// <returns></returns>
        ITaskOptionsBuilder AsOptional();
    }
}
