using System;

namespace ConductorSharp.Engine.Builders.Metadata;

[AttributeUsage(AttributeTargets.Class)]
public class WorkflowMetadataAttribute : Attribute
{
    public string OwnerApp { get; set; }
    public string OwnerEmail { get; set; }
    public string Description { get; set; }
    public Type FailureWorkflow { get; set; }
}
