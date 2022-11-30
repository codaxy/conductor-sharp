using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;
using static ConductorSharp.Engine.Util.Builders.Events;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public abstract class Workflow<TWorkflow, TInput, TOutput> : IConfigurableWorkflow
        where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        protected readonly WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> _builder;
        private WorkflowDefinition _workflowDefinition;
        public event LoadWorflow OnRegisterEvent;
        public event GetWorkflowDefinition OnGetDefinitionEvent;
        public event ResolveWorkflow OnResolveEvent;

        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public Workflow() : this(null) { }

        public Workflow(BuildConfiguration buildConfiguration)
        {
            _builder = new WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> { BuildConfiguration = buildConfiguration };

            OnRegisterEvent += _builder.OnRegister;
            OnResolveEvent += _builder.OnResolve;
            OnGetDefinitionEvent += _builder.OnGetDefinition;
        }

        public abstract void UpdateWorkflowDefinition();

        public WorkflowDefinition GetDefinition()
        {
            if (_workflowDefinition == null)
            {
                UpdateWorkflowDefinition();
                _workflowDefinition = _builder.Build();
            }

            return _workflowDefinition;
        }

        public void OnRegistration(ContainerBuilder containerBuilder)
        {
            OnRegisterEvent?.Invoke(containerBuilder);
        }

        public void OnResolve(IComponentContext componentContext)
        {
            OnResolveEvent?.Invoke(componentContext);
        }

        public void OnGetDefinition(WorkflowDefinition workflowDefinition)
        {
            OnGetDefinitionEvent?.Invoke(workflowDefinition);
        }
    }
}
