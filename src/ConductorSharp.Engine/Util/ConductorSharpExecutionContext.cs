using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    public class ConductorSharpExecutionContext
    {
        public string WorkflowName { get; set; }
        public string WorkflowId { get; set; }
        public string TaskName { get; set; }
        public string TaskId { get; set; }
        public string TaskReferenceName { get; set; }
        public string CorrelationId { get; set; }
        public string WorkerId { get; set; }
    }
}
