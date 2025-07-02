using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Service;
using ConductorSharp.Engine.Tests.Samples.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Tests.Integration
{
    public class UtilityTests
    {
        [Fact]
        public void NamingUtilShouldReturnCorrectNameableObjectName()
        {
            Assert.Equal("TEST_StringInterpolation", NamingUtil.NameOf<StringInterpolation>());
            Assert.Equal("CUSTOMER_get", NamingUtil.NameOf<CustomerGetV1>());
        }

        public class Request : ITaskInput<Request.Response>
        {
            public string Baba { get; set; } = "Baba";

            public class Response { }

            public class Handler : IWorker<Request, Response>
            {
                public Task<Response> Handle(Request request, WorkerExecutionContext context, CancellationToken cancellationToken)
                {
                    return Task.FromResult(new Response());
                }
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
                return await next();
            }
        }

        public class GenericMiddleware<TRequest, TResponse> : IWorkerMiddleware<TRequest, TResponse>
            where TRequest : class, ITaskInput<TResponse>, new()
        {
            public async Task<TResponse> Handle(
                TRequest request,
                WorkerExecutionContext context,
                Func<Task<TResponse>> next,
                CancellationToken cancellationToken
            )
            {
                return await next();
            }
        }

        [Fact]
        public async Task Test()
        {
            var collection = new ServiceCollection();
            collection.RegisterWorkerTask<Request.Handler>();
            collection.AddTransient<IWorkerMiddleware<Request, Request.Response>, Middleware>();
            collection.AddTransient(typeof(IWorkerMiddleware<,>), typeof(GenericMiddleware<,>));

            var provider = collection.BuildServiceProvider();
            var invoker = new WorkerInvokerService(provider);

            for (int i = 0; i < 5; i++)
            {
                var sw = Stopwatch.StartNew();
                var result = await invoker.Invoke(
                    typeof(Request.Handler),
                    new Dictionary<string, object>(),
                    new WorkerExecutionContext(WorkflowName: "test", WorkflowId: null, "", "", "", "", ""),
                    new CancellationToken(true)
                );
                var t = sw.ElapsedMilliseconds;
            }
        }
    }
}
