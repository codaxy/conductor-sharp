using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util.Builders;
using static ConductorSharp.Engine.Util.Builders.Events;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public abstract class Workflow<TWorkflow, TInput, TOutput> : ITypedWorkflow
        where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        private readonly WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> _builder;
        private readonly BuildConfiguration _buildConfiguration;
        private readonly BuildContext _buildContext;
        public event HandleRegistration HandlerRegistrations;
        public event HandleBuild HandleBuild;
        public event HandleResolve HandlerResolve;

        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public Workflow(BuildConfiguration buildConfiguration, BuildContext buildContext)
        {
            _builder = new WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput>(buildConfiguration, buildContext);
            HandlerRegistrations += _builder.HandleRegistration;
            HandlerResolve += _builder.HandleResolve;
            _buildConfiguration = buildConfiguration;
            _buildContext = buildContext;
        }

        public abstract void AddTasks(WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder);

        public WorkflowDefinition GetDefinition()
        {
            AddTasks(_builder);
            return _builder.Build(null);
        }

        public void OnRegistration(ContainerBuilder containerBuilder)
        {
            HandlerRegistrations?.Invoke(containerBuilder);
        }

        public void OnResolve(IComponentContext componentContext)
        {
            HandlerResolve?.Invoke(componentContext);
        }

        public void OnGetDefinition(WorkflowDefinition workflowDefinition)
        {
            HandleBuild?.Invoke(workflowDefinition);
        }
    }
}
