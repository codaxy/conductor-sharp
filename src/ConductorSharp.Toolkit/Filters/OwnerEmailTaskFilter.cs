using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerEmailTaskFilter : ITaskFilter
    {
        private readonly string _ownerEmail;

        public OwnerEmailTaskFilter(string ownerEmail) => _ownerEmail = ownerEmail;

        public bool Test(TaskDefinition taskDefinition) => taskDefinition.Name != null && taskDefinition.Name == _ownerEmail;
    }
}
