using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Interface
{
    public interface ITypedWorkflow : INameable
    {
        WorkflowDefinition GetDefinition();
        CSharpLambda[] Lambdas { get; }
    }
}
