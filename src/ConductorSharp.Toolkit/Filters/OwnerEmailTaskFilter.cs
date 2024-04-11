using ConductorSharp.Client.Generated;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerEmailTaskFilter : ITaskFilter
    {
        private readonly string[] _ownerEmails;

        public OwnerEmailTaskFilter(IEnumerable<string> ownerEmails) => _ownerEmails = ownerEmails.ToArray();

        public bool Test(TaskDef taskDefinition) =>
            taskDefinition.OwnerEmail != null && _ownerEmails.Any(email => email == taskDefinition.OwnerEmail);
    }
}
