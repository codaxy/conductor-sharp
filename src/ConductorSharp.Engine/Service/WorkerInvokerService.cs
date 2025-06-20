using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Engine.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace ConductorSharp.Engine.Service
{
    public class WorkerInvokerService(IServiceProvider serviceProvider)
    {
        private record WorkerTypeInfo
        {
            public WorkerTypeInfo(Type workerType)
            {
                WorkerType = workerType;
                (RequestType, ResponseType) = GetRequestResponseTypes(workerType);
                MiddlewareType = typeof(INgWorkerMiddleware<,>).MakeGenericType(RequestType, ResponseType);
                WorkerHandleMethod = typeof(INgWorker<,>)
                    .MakeGenericType(RequestType, ResponseType)
                    .GetMethod(nameof(INgWorker<object, object>.Handle));
                MiddlewareHandleMethod = MiddlewareType.GetMethod(nameof(INgWorkerMiddleware<object, object>.Handle));
                NextFuncType = MiddlewareHandleMethod!.GetParameters().FirstOrDefault(p => p.Name == "next")!.ParameterType;
                TaskResultProperty = typeof(Task<>).MakeGenericType(ResponseType).GetProperty(nameof(Task<object>.Result));
            }

            public Type WorkerType { get; }
            public Type RequestType { get; }
            public Type ResponseType { get; }
            public Type MiddlewareType { get; }
            public MethodInfo WorkerHandleMethod { get; }
            public MethodInfo MiddlewareHandleMethod { get; }
            public Type NextFuncType { get; }
            public PropertyInfo TaskResultProperty { get; }
        }

        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task<IDictionary<string, object>> Invoke(
            Type workerType,
            IDictionary<string, object> request,
            CancellationToken cancellationToken
        ) { }

        // TODO: MR Removal: Make private
        public async Task<object> InternalInvoke(Type workerType, object request, CancellationToken cancellationToken)
        {
            var workerTypeInfo = new WorkerTypeInfo(workerType);
            var middlewares = _serviceProvider.GetServices(workerTypeInfo.MiddlewareType).ToArray();
            var worker = _serviceProvider.GetRequiredService(workerType);
            var resultTask = InvokeMiddlewarePipeline(middlewares, worker, workerTypeInfo, request, cancellationToken);
            await resultTask;
            var result = workerTypeInfo.TaskResultProperty.GetValue(resultTask);
            return result;
        }

        private static (Type RequestType, Type ResponseType) GetRequestResponseTypes(Type workerType)
        {
            var types = workerType.GetInterface(typeof(INgWorker<,>).Name)!.GetGenericArguments();
            return (types[0], types[1]);
        }

        private static async Task<object> InvokeHandler(object worker, object request, CancellationToken cancellationToken)
        {
            var resultTask = (Task)worker.GetType().GetMethod(nameof(INgWorker<object, object>.Handle))!.Invoke(worker, [request, cancellationToken]);
            await resultTask!;

            var propInfo = resultTask.GetType().GetProperty(nameof(Task<object>.Result));
            var result = propInfo!.GetValue(resultTask);

            return Task.FromResult(result);
        }

        private static Task InvokeMiddlewarePipeline(
            object[] middlewares,
            object worker,
            WorkerTypeInfo workerTypeInfo,
            object request,
            CancellationToken cancellationToken
        )
        {
            object result;
            if (middlewares.Length == 0)
                result = workerTypeInfo.WorkerHandleMethod.Invoke(worker, [request, cancellationToken]);
            else
                result = workerTypeInfo.MiddlewareHandleMethod.Invoke(
                    middlewares[0],
                    [request, GenerateCallToMiddleware(middlewares, worker, 1, workerTypeInfo).Compile(), cancellationToken]
                );

            return (Task)result;
        }

        private static LambdaExpression GenerateCallToHandler(object worker, WorkerTypeInfo workerInfo)
        {
            var requestParam = Expression.Parameter(workerInfo.RequestType);
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken));

            var lambdaBody = Expression.Call(Expression.Constant(worker), workerInfo.WorkerHandleMethod, requestParam, cancellationTokenParam);
            var lambda = Expression.Lambda(workerInfo.NextFuncType, lambdaBody, requestParam, cancellationTokenParam);
            return lambda;
        }

        private static LambdaExpression GenerateCallToMiddleware(object[] middlewares, object worker, int middlewareIndex, WorkerTypeInfo workerInfo)
        {
            if (middlewares.Length == middlewareIndex)
                return GenerateCallToHandler(worker, workerInfo);

            var requestParam = Expression.Parameter(workerInfo.RequestType);
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken));

            var nextLambda = GenerateCallToMiddleware(middlewares, worker, middlewareIndex + 1, workerInfo);
            var lambdaBody = Expression.Call(
                Expression.Constant(middlewares[middlewareIndex]),
                workerInfo.MiddlewareHandleMethod,
                requestParam,
                nextLambda,
                cancellationTokenParam
            );
            var lambda = Expression.Lambda(workerInfo.NextFuncType, lambdaBody, requestParam, cancellationTokenParam);
            return lambda;
        }
    }
}
