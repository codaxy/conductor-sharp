using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;

namespace ConductorSharp.Engine.Builders
{
    public class WorkflowInput<T> : IWorkflowInput, IRequest<T> where T : WorkflowOutput { }

    public class WorkflowOutput { }

    public class WorkflowId { }

    public interface IWorkflowInput { }

    public abstract class Workflow<TWorkflow, TInput, TOutput> : SubWorkflowTaskModel<TInput, TOutput>, IConfigurableWorkflow
        where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        protected WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> _builder;
        private WorkflowDefinition _workflowDefinition;

        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public abstract void BuildDefinition();

        public Workflow(WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder)
        {
            _builder = builder;
        }

        public WorkflowDefinition GetDefinition()
        {
            return _builder.Build();
        }
    }
}
