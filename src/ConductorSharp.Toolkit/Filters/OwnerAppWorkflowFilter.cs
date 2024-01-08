using ConductorSharp.Client.Generated;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerAppWorkflowFilter : IWorkflowFilter
    {
        private readonly string[] _ownerApps;

        public OwnerAppWorkflowFilter(IEnumerable<string> ownerApps) => _ownerApps = ownerApps.ToArray();

        public bool Test(WorkflowDef workflowDefinition) =>
            workflowDefinition.OwnerApp != null && _ownerApps.Any(app => app == workflowDefinition.OwnerApp);
    }
}
