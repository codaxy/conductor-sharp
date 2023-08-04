using ConductorSharp.Client.Model.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ConductorSharp.Client.Model.Request
{
    public partial class WorkflowTestRequest
    {
        public partial class TaskMock
        {
            public enum TaskMockStatus
            {
                [System.Runtime.Serialization.EnumMember(Value = @"IN_PROGRESS")]
                IN_PROGRESS = 0,

                [System.Runtime.Serialization.EnumMember(Value = @"FAILED")]
                FAILED = 1,

                [System.Runtime.Serialization.EnumMember(Value = @"FAILED_WITH_TERMINAL_ERROR")]
                FAILED_WITH_TERMINAL_ERROR = 2,

                [System.Runtime.Serialization.EnumMember(Value = @"COMPLETED")]
                COMPLETED = 3,
            }

            public TaskMockStatus Status { get; set; }
            Dictionary<string, object> Output { get; set; }

            public long ExecutionTime { get; set; }

            public long QueueWaitTime { get; set; }

            Dictionary<string, object> _additionalProperties;

            public Dictionary<string, object> AdditionalProperties
            {
                get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
                set { _additionalProperties = value; }
            }
        }

        public string Name { get; set; }

        public int Version { get; set; }

        public string CorrelationId { get; set; }

        public Dictionary<string, object> Input { get; set; }

        public Dictionary<string, string> TaskToDomain { get; set; }

        public WorkflowDefinition WorkflowDef { get; set; }

        public string ExternalInputPayloadStoragePath { get; set; }

        public int Priority { get; set; }

        public Dictionary<string, Collection<TaskMock>> TaskRefToMockOutput { get; set; }

        public Dictionary<string, WorkflowTestRequest> SubWorkflowTestRequest { get; set; }

        private Dictionary<string, object> _additionalProperties;

        public Dictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
            set { _additionalProperties = value; }
        }
    }
}
