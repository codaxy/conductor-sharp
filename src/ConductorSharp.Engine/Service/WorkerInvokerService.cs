using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using ConductorSharp.Client;
using ConductorSharp.Client.Util;
using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
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
                (RequestType, ResponseType) = WorkerUtil.GetRequestResponseTypes(workerType);
                MiddlewareType = typeof(INgWorkerMiddleware<,>).MakeGenericType(RequestType, ResponseType);
                WorkerHandleMethod = typeof(INgWorker<,>)
                    .MakeGenericType(RequestType, ResponseType)
                    .GetMethod(nameof(INgWorker<ObjectRequest, object>.Handle));
                MiddlewareHandleMethod = MiddlewareType.GetMethod(nameof(INgWorkerMiddleware<ObjectRequest, object>.Handle));
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
            WorkerExecutionContext workerExecutionContext,
            CancellationToken cancellationToken
        )
        {
            var workerTypeInfo = new WorkerTypeInfo(workerType);
            var objRequest = SerializationHelper.DictonaryToObject(workerTypeInfo.RequestType, request, ConductorConstants.IoJsonSerializerSettings);
            var objResponse = await InternalInvoke(workerTypeInfo, objRequest, workerExecutionContext, cancellationToken);
            var response = SerializationHelper.ObjectToDictionary(objResponse, ConductorConstants.IoJsonSerializerSettings);

            return response;
        }

        private async Task<object> InternalInvoke(
            WorkerTypeInfo workerTypeInfo,
            object request,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            var middlewares = _serviceProvider.GetServices(workerTypeInfo.MiddlewareType).ToArray();
            var worker = _serviceProvider.GetRequiredService(workerTypeInfo.WorkerType);
            var resultTask = InvokeMiddlewarePipeline(middlewares, worker, workerTypeInfo, request, context, cancellationToken);
            await resultTask;
            var result = workerTypeInfo.TaskResultProperty.GetValue(resultTask);
            return result;
        }

        private static Task InvokeMiddlewarePipeline(
            object[] middlewares,
            object worker,
            WorkerTypeInfo workerTypeInfo,
            object request,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            object result;
            if (middlewares.Length == 0)
                result = workerTypeInfo.WorkerHandleMethod.Invoke(worker, [request, context, cancellationToken]);
            else
            {
                var next = GenerateCallToMiddleware(middlewares, worker, request, 1, workerTypeInfo, context, cancellationToken).Compile();
                result = workerTypeInfo.MiddlewareHandleMethod.Invoke(middlewares[0], [request, context, next, cancellationToken]);
            }

            return (Task)result;
        }

        private static LambdaExpression GenerateCallToHandler(
            object worker,
            object request,
            WorkerTypeInfo workerInfo,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            var requestArgument = Expression.Constant(request);
            var cancellationTokenArgument = Expression.Constant(cancellationToken);
            var contextArgument = Expression.Constant(context);

            var lambdaBody = Expression.Call(
                Expression.Constant(worker),
                workerInfo.WorkerHandleMethod,
                requestArgument,
                contextArgument,
                cancellationTokenArgument
            );
            var lambda = Expression.Lambda(workerInfo.NextFuncType, lambdaBody);
            return lambda;
        }

        private static LambdaExpression GenerateCallToMiddleware(
            object[] middlewares,
            object worker,
            object request,
            int middlewareIndex,
            WorkerTypeInfo workerInfo,
            WorkerExecutionContext context,
            CancellationToken cancellationToken
        )
        {
            if (middlewares.Length == middlewareIndex)
                return GenerateCallToHandler(worker, request, workerInfo, context, cancellationToken);

            var requestArgument = Expression.Constant(request);
            var cancellationTokenArgument = Expression.Constant(cancellationToken);
            var contextArgument = Expression.Constant(context);

            var nextLambda = GenerateCallToMiddleware(middlewares, worker, request, middlewareIndex + 1, workerInfo, context, cancellationToken);
            var lambdaBody = Expression.Call(
                Expression.Constant(middlewares[middlewareIndex]),
                workerInfo.MiddlewareHandleMethod,
                requestArgument,
                contextArgument,
                nextLambda,
                cancellationTokenArgument
            );
            var lambda = Expression.Lambda(workerInfo.NextFuncType, lambdaBody);
            return lambda;
        }
    }
}
