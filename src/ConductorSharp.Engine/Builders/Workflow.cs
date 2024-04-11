using ConductorSharp.Client.Generated;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using MediatR;

namespace ConductorSharp.Engine.Builders
{
    public class WorkflowInput<T> : IWorkflowInput, IRequest<T>
        where T : WorkflowOutput { }

    public class WorkflowOutput { }

    public class WorkflowId { }

    public interface IWorkflowInput { }

    public abstract class Workflow<TWorkflow, TInput, TOutput>(WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder)
        : SubWorkflowTaskModel<TInput, TOutput>,
            IConfigurableWorkflow
        where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        protected WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> _builder = builder;
        private WorkflowDef _workflowDefinition;

        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public virtual void BuildDefinition() { }

        public virtual WorkflowDef GetDefinition()
        {
            if (_workflowDefinition == null)
            {
                BuildDefinition();
                _workflowDefinition = _builder.Build();
            }

            return _workflowDefinition;
        }
    }
}
