using ConductorSharp.Client;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Service
{
    internal class WorkflowStarterService : IWorkflowStarterService
    {
        private const string WorkflowIdInputName = $"ConductorSharp.Engine.{nameof(WorkflowIdInputName)}";
        private const string MachineIdentifierInputName = $"ConductorSharp.Engine.{nameof(MachineIdentifierInputName)}";

        private readonly IWorkflowService _workflowService;
        private readonly WorkflowTaskSourcesService _workflowTaskSourcesService;
        private readonly ListenerConfiguration _listenerConfiguration;

        public WorkflowStarterService(
            IWorkflowService workflowService,
            WorkflowTaskSourcesService workflowTaskSourcesService,
            ListenerConfiguration listenerConfiguration
        )
        {
            _workflowService = workflowService;
            _workflowTaskSourcesService = workflowTaskSourcesService;
            _listenerConfiguration = listenerConfiguration;
        }

        public async Task<TOutput> StartWorkflowAsync<TWorkflow, TInput, TOutput>(TInput input)
            where TWorkflow : Workflow<TWorkflow, TInput, TOutput>
            where TInput : WorkflowInput<TOutput>
            where TOutput : WorkflowOutput
        {
            var result = await StartWorkflowAsync(NamingUtil.NameOf<TWorkflow>(), 1, JObject.FromObject(input, ConductorConstants.IoJsonSerializer));
            return result.ToObject<TOutput>(ConductorConstants.IoJsonSerializer);
        }

        public async Task<JObject> StartWorkflowAsync(string workflowName, int version, JObject input)
        {
            var workflowInputId = Guid.NewGuid().ToString();
            var task = _workflowTaskSourcesService.AllocateTask(workflowInputId);
            var inputObj = new JObject(input)
            {
                { WorkflowIdInputName, workflowInputId },
                { MachineIdentifierInputName, _listenerConfiguration.MachineIdentifier }
            };
            await _workflowService.QueueWorkflowStringResponse(workflowName, 1, inputObj);
            return await task;
        }
    }
}
