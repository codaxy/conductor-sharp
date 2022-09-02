using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerEmailWorkflowFilter : IWorkflowFilter
    {
        private readonly string _ownerEmail;

        public OwnerEmailWorkflowFilter(string ownerEmail) => _ownerEmail = ownerEmail;

        public bool Test(WorkflowDefinition workflowDefinition) =>
            workflowDefinition.OwnerEmail != null || workflowDefinition.OwnerEmail == _ownerEmail;
    }
}
