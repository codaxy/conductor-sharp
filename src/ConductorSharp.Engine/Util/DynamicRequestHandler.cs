using ConductorSharp.Engine.Interface;
using MediatR;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Util
{
    public class DynamicRequestHandler<TRequestProxy, TRequest, TResponse> : IRequestHandler<TRequestProxy, TResponse>, IDynamicHandler
        where TRequestProxy : IRequest<TResponse>
        where TRequest : new()
    {
        public Func<TRequest, TResponse> _handlerFunc;

        public Task<TResponse> Handle(TRequestProxy request, CancellationToken cancellationToken) =>
            Task.FromResult(_handlerFunc(MapRequest(request)));

        private TRequest MapRequest(TRequestProxy requestProxy)
        {
            var proxyProperties = typeof(TRequestProxy).GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            var requestProperties = typeof(TRequest).GetProperties().Where(prop => prop.CanRead && prop.CanWrite).ToDictionary(prop => prop.Name);
            var request = new TRequest();

            foreach (var proxyProp in proxyProperties)
                requestProperties[proxyProp.Name].SetValue(request, proxyProp.GetValue(requestProxy));

            return request;
        }
    }
}
