using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerAppWorkflowFilter : IWorkflowFilter
    {
        private readonly string _ownerApp;

        public OwnerAppWorkflowFilter(string ownerApp) => _ownerApp = ownerApp;

        public bool Test(WorkflowDefinition workflowDefinition) => workflowDefinition.OwnerApp != null && workflowDefinition.OwnerApp == _ownerApp;
    }
}
