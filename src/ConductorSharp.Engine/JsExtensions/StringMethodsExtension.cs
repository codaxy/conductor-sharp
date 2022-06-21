using Lambda2Js;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.JsExtensions
{
    internal class StringMethodsExtension : TypeMethodsExtension<string>
    {
        public override void HandleMethodExpression(JavascriptConversionContext context, MethodCallExpression expression)
        {
            context.PreventDefault();

            if (expression.Object == null)
                throw new NotSupportedException("Static methods not supported");

            ParameterInfo[] @params = expression.Method.GetParameters();

            switch (expression.Method.Name)
            {

                case nameof(string.Replace):
                    {
                        context.Write(expression.Object);
                        if (@params.Length != 2)
                            throw new NotSupportedException($"{nameof(string.Replace)} method with more than 2 parameters not supported");

                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".replaceAll");
                        context.WriteManyIsolated('(', ')', ',', expression.Arguments);
                    }
                    break;

                case nameof(string.ToLowerInvariant):
                    context.Write(expression.Object);
                    context.Write(".toLowerCase()");
                    break;
                default:
                    throw new NotSupportedException($"{expression.Method.Name} not supported");
            }
        }
    }
}
