using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    public class ConductorSharpExecutionContext
    {
        private static readonly IReadOnlyDictionary<string, string> NoContext = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>()
        );

        public string WorkflowName { get; set; }
        public string WorkflowId { get; set; }
        public string TaskName { get; set; }
        public string TaskId { get; set; }
        public string TaskReferenceName { get; set; }
        public string CorrelationId { get; set; }
        public string WorkerId { get; set; }

        /// <summary>
        /// The execution context of the workflow run the task belongs to: key/value pairs the run was
        /// started with, such as the tenant and user it runs for. Maestro sends it with every polled
        /// task. Conductor sends none, and then this is empty.
        /// </summary>
        /// <remarks>
        /// Read identity from here rather than from the task's input. Maestro does not let the
        /// workflow change the context after the start, while the input is whatever the workflow
        /// definition puts there.
        /// </remarks>
        public IReadOnlyDictionary<string, string> Context { get; set; } = NoContext;

        /// <summary>
        /// Copies the context from a polled task, so a handler cannot change what the next handler
        /// sees by writing to the task's dictionary.
        /// </summary>
        internal static IReadOnlyDictionary<string, string> CopyContext(IDictionary<string, string> context)
        {
            if (context == null || context.Count == 0)
                return NoContext;

            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(context));
        }
    }
}
