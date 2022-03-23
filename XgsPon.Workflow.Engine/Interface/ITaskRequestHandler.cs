using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XgsPon.Workflows.Client.Model.Response;
using static XgsPon.Workflows.Client.Model.Request.UpdateTaskRequest;

namespace XgsPon.Workflows.Engine.Interface
{
    public interface ITaskRequestHandler<TRequest, TResponse>
        : IWorker,
          IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
    }
}
