using ConductorSharp.Client.Generated;

namespace ConductorSharp.Toolkit.Filters
{
    public interface ITaskFilter
    {
        bool Test(TaskDef taskDefinition);
    }
}
