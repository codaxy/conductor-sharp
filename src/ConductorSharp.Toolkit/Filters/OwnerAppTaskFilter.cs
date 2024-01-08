using ConductorSharp.Client.Generated;

namespace ConductorSharp.Toolkit.Filters
{
    public class OwnerAppTaskFilter : ITaskFilter
    {
        private readonly string[] _ownerApps;

        public OwnerAppTaskFilter(IEnumerable<string> ownerApps) => _ownerApps = ownerApps.ToArray();

        public bool Test(TaskDef taskDefinition) => taskDefinition.OwnerApp != null && _ownerApps.Any(app => app == taskDefinition.OwnerApp);
    }
}
