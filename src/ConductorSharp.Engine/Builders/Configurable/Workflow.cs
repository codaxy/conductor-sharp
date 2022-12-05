﻿using Autofac;
using ConductorSharp.Client.Model.Common;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using ConductorSharp.Engine.Util.Builders;

namespace ConductorSharp.Engine.Builders.Configurable
{
    public abstract class Workflow<TWorkflow, TInput, TOutput> : IConfigurableWorkflow
        where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        protected WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> _builder;
        private WorkflowDefinition _workflowDefinition;
        public event GetWorkflowDefinition OnGetDefinitionEvent;
        public event ResolveWorkflow OnResolveEvent;

        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public abstract void BuildDefinition();

        public Workflow(WorkflowDefinitionBuilder<TWorkflow, TInput, TOutput> builder)
        {
            _builder = builder;

            OnResolveEvent += _builder.OnResolve;
            OnGetDefinitionEvent += _builder.OnGetDefinition;
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

        public void BeforeGetDefinition(IComponentContext componentContext, BuildConfiguration buildConfiguration)
        {
            OnResolveEvent?.Invoke(componentContext);
        }

        public void OnGetDefinition(WorkflowDefinition workflowDefinition)
        {
            OnGetDefinitionEvent?.Invoke(workflowDefinition);
        }
    }
}
