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

        public class Request
        {
            public int Counter = 0;

            public class Response { }

            public class Handler : INgWorker<Request, Response>
            {
                public Task<Response> Handle(Request request, CancellationToken cancellationToken)
                {
                    request.Counter++;
                    return Task.FromResult(new Response());
                }
            }
        }

        public class Middleware : INgWorkerMiddleware<Request, Request.Response>
        {
            public async Task<Request.Response> Handle(
                Request request,
                Func<Request, CancellationToken, Task<Request.Response>> next,
                CancellationToken cancellationToken
            )
            {
                request.Counter++;
                return await next(request, cancellationToken);
            }
        }

        public class GenericMiddleware<TRequest, TResponse> : INgWorkerMiddleware<TRequest, TResponse>
            where TResponse : class, new()
            where TRequest : class, new()
        {
            public async Task<TResponse> Handle(
                TRequest request,
                Func<TRequest, CancellationToken, Task<TResponse>> next,
                CancellationToken cancellationToken
            )
            {
                var obj = (object)request;
                var req = (Request)obj;
                req.Counter++;
                return await next(request, cancellationToken);
            }
        }

        [Fact]
        public async Task Test()
        {
            var collection = new ServiceCollection();
            collection.RegisterWorkerTask<Request.Handler>();
            collection.AddTransient<INgWorkerMiddleware<Request, Request.Response>, Middleware>();
            collection.AddTransient(typeof(INgWorkerMiddleware<,>), typeof(GenericMiddleware<,>));

            var provider = collection.BuildServiceProvider();
            var invoker = new WorkerInvokerService(provider);

            var sw = Stopwatch.StartNew();
            var @ref = new Request();
            var result = await invoker.Invoke(typeof(Request.Handler), @ref, default);
            var response = (Request.Response)result;
            var t = sw.ElapsedMilliseconds;
        }
    }
}
