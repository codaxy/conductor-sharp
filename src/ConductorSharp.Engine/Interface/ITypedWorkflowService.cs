using ConductorSharp.Client;
using ConductorSharp.Client.Service;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Model;
using ConductorSharp.Engine.Util;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Interface
{
    public interface ITypedWorkflowService
    {
        Task<string> StartWorkflow<TWorkflow, TInput, TOutput>(TInput input)
            where TWorkflow : SubWorkflowTaskModel<TInput, TOutput>
            where TInput : IRequest<TOutput>;
    }
}
