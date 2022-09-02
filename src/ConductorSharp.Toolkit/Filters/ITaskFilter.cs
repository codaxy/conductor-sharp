using ConductorSharp.Client.Model.Common;

namespace ConductorSharp.Toolkit.Filters
{
    public interface ITaskFilter
    {
        bool Test(TaskDefinition taskDefinition);
    }
}
