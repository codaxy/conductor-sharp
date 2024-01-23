using ConductorSharp.Client.Generated;

namespace ConductorSharp.Toolkit.Filters
{
    public interface IWorkflowFilter
    {
        bool Test(WorkflowDef workflowDefinition);
    }
}
