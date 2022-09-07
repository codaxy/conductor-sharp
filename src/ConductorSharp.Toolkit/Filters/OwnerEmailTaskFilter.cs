using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerEmailTaskFilter : ITaskFilter
    {
        private readonly string[] _ownerEmails;

        public OwnerEmailTaskFilter(IEnumerable<string> ownerEmails) => _ownerEmails = ownerEmails.ToArray();

        public bool Test(TaskDefinition taskDefinition) =>
            taskDefinition.OwnerEmail != null && _ownerEmails.Any(email => email == taskDefinition.OwnerEmail);
    }
}
