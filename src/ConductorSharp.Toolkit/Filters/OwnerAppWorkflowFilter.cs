using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerAppWorkflowFilter : IWorkflowFilter
    {
        private readonly string[] _ownerApps;

        public OwnerAppWorkflowFilter(IEnumerable<string> ownerApps) => _ownerApps = ownerApps.ToArray();

        public bool Test(WorkflowDefinition workflowDefinition) =>
            workflowDefinition.OwnerApp != null && _ownerApps.Any(app => app == workflowDefinition.OwnerApp);
    }
}
