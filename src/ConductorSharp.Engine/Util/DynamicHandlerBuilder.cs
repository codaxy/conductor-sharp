using ConductorSharp.Engine.Interface;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Util
{
    internal class DynamicHandlerBuilder
    {
        private const string LambdaFieldName = "_handlerFunc";
        internal static DynamicHandlerBuilder DefaultBuilder { get; } = new DynamicHandlerBuilder("ConductorSharp.DynamicHandlers");

        internal class DynamicHandler
        {
            private readonly Type _type;
            private readonly Delegate _handlerFunc;

            public DynamicHandler(Type type, Delegate handlerFunc)
            {
                _type = type;
                _handlerFunc = handlerFunc;
            }

            public object CreateInstance()
            {
                var obj = Activator.CreateInstance(_type);
                _type.GetField(LambdaFieldName).SetValue(obj, _handlerFunc);
                return obj;
            }
        }

        private readonly ModuleBuilder _moduleBuilder;
        private readonly List<DynamicHandler> _handlers = new();

        public ReadOnlyCollection<DynamicHandler> Handlers => _handlers.AsReadOnly();

        private DynamicHandlerBuilder(string assemblyName)
        {
            var assemblyNameObj = new AssemblyName(assemblyName);
            _moduleBuilder = AssemblyBuilder
                .DefineDynamicAssembly(assemblyNameObj, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(assemblyNameObj.Name);
        }

        public void AddDynamicHandler<TInput, TOutput>(Func<TInput, TOutput> handlerFunc, string taskName) where TInput : IRequest<TOutput>
        {
            var typeBuilder = _moduleBuilder.DefineType(
                Guid.NewGuid().ToString(),
                TypeAttributes.Public,
                null,
                new[] { typeof(ITaskRequestHandler<TInput, TOutput>), typeof(IDynamicHandler) }
            );
            var attributeBuilder = new CustomAttributeBuilder(
                typeof(OriginalNameAttribute).GetConstructor(new[] { typeof(string) }),
                new[] { taskName }
            );
            typeBuilder.SetCustomAttribute(attributeBuilder);
            var handlerFuncField = typeBuilder.DefineField(LambdaFieldName, handlerFunc.GetType(), FieldAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(IRequestHandler<TInput, TOutput>.Handle),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(Task<TOutput>),
                new[] { typeof(TInput), typeof(CancellationToken) }
            );
            GenerateMethodIL<TInput, TOutput>(methodBuilder, handlerFuncField);
            _handlers.Add(new DynamicHandler(typeBuilder.CreateType(), handlerFunc));
        }

        private void GenerateMethodIL<TInput, TOutput>(MethodBuilder methodBuilder, FieldBuilder handlerFuncField)
        {
            var ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, handlerFuncField);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(Func<TInput, TOutput>).GetMethod(nameof(Func<TInput, TOutput>.Invoke)));
            ilGenerator.Emit(OpCodes.Call, typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(typeof(TOutput)));
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
