using ConductorSharp.Client;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    public class TypedWorkflowService : ITypedWorkflowService
    {
        private readonly IWorkflowService _workflowService;

        public TypedWorkflowService(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        public async Task<string> StartWorkflow<TWorkflow, TInput, TOutput>(TInput input)
            where TWorkflow : SubWorkflowTaskModel<TInput, TOutput>
            where TInput : IRequest<TOutput>
        {
            var version = typeof(TWorkflow).GetCustomAttribute<VersionAttribute>()?.Version ?? 1;
            var name = NamingUtil.NameOf<TWorkflow>();
            var workflowInput = JObject.FromObject(input, ConductorConstants.IoJsonSerializer);

            return await _workflowService.QueueWorkflowStringResponse(name, version, workflowInput);
        }
    }
}
