using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public interface IWorkflowFilter
    {
        bool Test(WorkflowDefinition workflowDefinition);
    }
}
