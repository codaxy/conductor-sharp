using System;
using ConductorSharp.Engine.Interface;

namespace ConductorSharp.Engine.Util
{
    internal static class WorkerUtil
    {
        public static (Type RequestType, Type ResponseType) GetRequestResponseTypes(Type workerType)
        {
            var types = workerType.GetInterface(typeof(IWorker<,>).Name)!.GetGenericArguments();
            return (types[0], types[1]);
        }
    }
}
