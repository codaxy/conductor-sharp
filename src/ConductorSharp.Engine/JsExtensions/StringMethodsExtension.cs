using ConductorSharp.Engine.Util;
using Lambda2Js;
using System;
using System.Linq;
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
                        if (@params.Length != 2)
                            throw new NotSupportedException($"{nameof(string.Replace)} method with more than 2 parameters not supported");

                        context.Write(expression.Object);
                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".replaceAll");
                        context.WriteManyIsolated('(', ')', ',', expression.Arguments);
                    }
                    break;

                case nameof(string.ToLowerInvariant):
                    {
                        context.Write(expression.Object);
                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".toLowerCase()");
                    }
                    break;

                case nameof(string.ToUpperInvariant):
                    {
                        context.Write(expression.Object);
                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".toUpperCase()");
                    }
                    break;

                case nameof(string.Trim):
                    {
                        if (@params.Length == 0)
                        {
                            context.Write(expression.Object);
                            using var op = context.Operation(JavascriptOperationTypes.Call);
                            context.Write(".trim()");
                        }
                        else
                        {
                            var script = EmbeddedFileHelper.GetLinesFromEmbeddedFile("~/trim.js");
                            using var op = context.Operation(JavascriptOperationTypes.Call);
                            context.Write(script);
                            context.WriteManyIsolated('(', ')', ',', new[] { expression.Object }.Concat(expression.Arguments));
                        }
                    }
                    break;

                case nameof(string.TrimStart):
                    {
                        if (@params.Length != 0)
                            throw new NotSupportedException($"{nameof(string.TrimStart)} with parameters are not supported");

                        context.Write(expression.Object);
                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".trimStart()");
                    }
                    break;

                case nameof(string.TrimEnd):
                    {
                        if (@params.Length != 0)
                            throw new NotSupportedException($"{nameof(string.TrimEnd)} with parameters are not supported");

                        context.Write(expression.Object);
                        using var op = context.Operation(JavascriptOperationTypes.Call);
                        context.Write(".trimEnd()");
                    }
                    break;
                default:
                    throw new NotSupportedException($"{expression.Method.Name} not supported");
            }
        }
    }
}
