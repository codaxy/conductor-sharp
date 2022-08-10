using ConductorSharp.Engine.Interface;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            var proxyInputType = GenerateProxyInputType<TInput, TOutput>(taskName);
            var requestHandlerType = typeof(DynamicRequestHandler<,,>).MakeGenericType(new[] { proxyInputType, typeof(TInput), typeof(TOutput) });
            var typeBuilder = _moduleBuilder.DefineType(taskName + "RequestHandler", TypeAttributes.Public, requestHandlerType);
            var attributeBuilder = new CustomAttributeBuilder(
                typeof(OriginalNameAttribute).GetConstructor(new[] { typeof(string) }),
                new[] { taskName }
            );
            typeBuilder.SetCustomAttribute(attributeBuilder);

            _handlers.Add(new DynamicHandler(typeBuilder.CreateType(), handlerFunc));
        }

        private Type GenerateProxyInputType<TInput, TOutput>(string taskName)
        {
            var inputType = typeof(TInput);
            var typeBuilder = _moduleBuilder.DefineType(
                taskName + inputType.Name + "Proxy",
                TypeAttributes.Public,
                null,
                new[] { typeof(IRequest<TOutput>) }
            );

            var inputProperties = inputType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var property in inputProperties)
                CreateProxyProperty(typeBuilder, property);

            return typeBuilder.CreateType();
        }

        private FieldBuilder GenerateField(TypeBuilder typeBuilder, PropertyInfo property) =>
            typeBuilder.DefineField($"_{property.Name}", property.PropertyType, FieldAttributes.Private);

        private void CreateProxyProperty(TypeBuilder typeBuilder, PropertyInfo property)
        {
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
            var fieldBuilder = GenerateField(typeBuilder, property);
            var getMethod = GenerateGetMethod(typeBuilder, property, fieldBuilder);
            var setMethod = GenerateSetMethod(typeBuilder, property, fieldBuilder);
            propertyBuilder.SetGetMethod(getMethod);
            propertyBuilder.SetSetMethod(setMethod);
            var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();

            if (attribute != null)
            {
                var attributeBuilder = new CustomAttributeBuilder(
                    typeof(JsonPropertyAttribute).GetConstructor(new[] { typeof(string) }),
                    new[] { attribute.PropertyName }
                );
                propertyBuilder.SetCustomAttribute(attributeBuilder);
            }
        }

        private MethodBuilder GenerateGetMethod(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                $"get_{property.Name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                property.PropertyType,
                null
            );

            var ilGen = methodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, fieldBuilder);
            ilGen.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private MethodBuilder GenerateSetMethod(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                $"set_{property.Name}",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                typeof(void),
                new[] { property.PropertyType }
            );

            var ilGen = methodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, fieldBuilder);
            ilGen.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }
}
