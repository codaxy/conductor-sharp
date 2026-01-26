using ConductorSharp.Client;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Service;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Tests.Unit;

public class WorkerInvokerServiceTests
{
    public class Request : ITaskInput<Request.Response>
    {
        public string Input { get; set; }

        public class Response
        {
            public string Output { get; set; }
            public WorkerExecutionContext HandlerContext { get; set; }
            public WorkerExecutionContext MiddlewareContext { get; set; }
            public WorkerExecutionContext GenericMiddlewareContext { get; set; }
        }

        public class Handler : IWorker<Request, Response>
        {
            public Task<Response> Handle(Request request, WorkerExecutionContext context, CancellationToken cancellationToken) =>
                Task.FromResult(new Response { Output = request.Input + "Worker", HandlerContext = context });
        }
    }

    public class Middleware : IWorkerMiddleware<Request, Request.Response>
    {
        public async Task<Request.Response> Handle(
            Request request,
            WorkerExecutionContext context,
            Func<Task<Request.Response>> next,
            CancellationToken cancellationToken
        )
        {
            var response = await next();
            response.MiddlewareContext = context;
            response.Output += "Middleware";
            return response;
        }
    }

    public class GenericMiddleware<TRequest, TResponse> : IWorkerMiddleware<TRequest, TResponse>
        where TRequest : ITaskInput<TResponse>, new()
    {
        public async Task<TResponse> Handle(
            TRequest request,
            WorkerExecutionContext context,
            Func<Task<TResponse>> next,
            CancellationToken cancellationToken
        )
        {
            var response = await next();
            var resp = (Request.Response)(object)response;
            resp.GenericMiddlewareContext = context;
            resp.Output += "GenericMiddleware";

            return response;
        }
    }

    [Fact]
    public async Task InvokerShouldInvokeWorkerAndMiddlewaresInCorrectOrder()
    {
        var collection = new ServiceCollection();
        collection
            .AddConductorSharp(baseUrl: "http://empty/empty")
            .AddExecutionManager(maxConcurrentWorkers: 1, sleepInterval: 1, longPollInterval: 1, domain: null)
            .AddPipelines(pipelineBuilder =>
            {
                pipelineBuilder.AddCustomMiddleware<Middleware, Request, Request.Response>();
                pipelineBuilder.AddCustomMiddleware(typeof(GenericMiddleware<,>));
            });

        collection.RegisterWorkerTask<Request.Handler>();

        var provider = collection.BuildServiceProvider();

        var invoker = new WorkerInvokerService(provider);
        var context = new WorkerExecutionContext(
            WorkflowName: "WorkflowName",
            WorkflowId: "WorkflowId",
            TaskName: "TaskName",
            TaskId: "TaskId",
            TaskReferenceName: "TaskReferenceName",
            CorrelationId: "CorrelationId",
            WorkerId: "WorkerId"
        );

        var expectedContext = context with { };
        var result = await invoker.Invoke(
            typeof(Request.Handler),
            new Dictionary<string, object>() { { "input", "Input" } },
            context,
            new CancellationToken(true)
        );

        Assert.Equal("InputWorkerGenericMiddlewareMiddleware", result["output"]);
        Assert.Equal(expectedContext, ((JObject)result["handler_context"]).ToObject<WorkerExecutionContext>(ConductorConstants.IoJsonSerializer));
        Assert.Equal(expectedContext, ((JObject)result["middleware_context"]).ToObject<WorkerExecutionContext>(ConductorConstants.IoJsonSerializer));
        Assert.Equal(
            expectedContext,
            ((JObject)result["generic_middleware_context"]).ToObject<WorkerExecutionContext>(ConductorConstants.IoJsonSerializer)
        );
    }
}
