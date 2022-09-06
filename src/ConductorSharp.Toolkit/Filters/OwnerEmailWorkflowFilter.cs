using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerEmailWorkflowFilter : IWorkflowFilter
    {
        private readonly string[] _ownerEmails;

        public OwnerEmailWorkflowFilter(IEnumerable<string> ownerEmails) => _ownerEmails = ownerEmails.ToArray();

        public bool Test(WorkflowDefinition workflowDefinition) =>
            workflowDefinition.OwnerEmail != null && _ownerEmails.Any(email => email == workflowDefinition.OwnerEmail);
    }
}
