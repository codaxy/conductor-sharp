using Autofac;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Builders
{
    public class WorkflowInput<T> : IWorkflowInput where T : WorkflowOutput
    {
    }

    public class WorkflowOutput
    {
    }

    public class WorkflowId
    {
    }

    public interface IWorkflowInput
    {
    }
    public abstract class Workflow<TInput, TOutput> : ITypedWorkflow
        where TInput : WorkflowInput<TOutput>
        where TOutput : WorkflowOutput
    {
        public TInput WorkflowInput { get; set; }
        public TOutput WorkflowOutput { get; set; }
        public WorkflowId Id { get; set; }

        public abstract WorkflowDefinition GetDefinition();
    }
}
