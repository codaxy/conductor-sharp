using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
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

        public virtual void BuildDefinition() { }

        public Workflow(WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder)
        {
            _builder = builder;
        }

        public virtual WorkflowDefinition GetDefinition()
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
