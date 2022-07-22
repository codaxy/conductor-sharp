using ConductorSharp.Engine.Util;
using Lambda2Js;
using System;
using System.Collections.Generic;
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
                    if (@params.Length != 2)
                        throw new NotSupportedException($"{nameof(string.Replace)} method with more than 2 parameters is not supported");

                    context.WriteMethodCall(expression.Object, "replaceAll", expression.Arguments);
                    break;

                case nameof(string.ToLowerInvariant):
                    context.WriteMethodCall(expression.Object, "toLowerCase", expression.Arguments);
                    break;

                case nameof(string.ToUpperInvariant):
                    context.WriteMethodCall(expression.Object, "toUpperCase", expression.Arguments);
                    break;

                case nameof(string.Trim):
                    if (@params.Length == 0)
                        context.WriteMethodCall(expression.Object, "trim", expression.Arguments);
                    else
                        context.WritePolyfillFunction("trim.js", new[] { expression.Object }.Concat(expression.Arguments));
                    break;

                case nameof(string.TrimStart):

                    {
                        IEnumerable<Expression> arguments = new[] { Expression.Constant(" ") };
                        if (@params.Length != 0)
                            arguments = expression.Arguments;

                        context.WritePolyfillFunction("trim.js", new[] { expression.Object }.Concat(arguments).Append(Expression.Constant("start")));
                    }
                    break;

                case nameof(string.TrimEnd):

                    {
                        IEnumerable<Expression> arguments = new[] { Expression.Constant(" ") };
                        if (@params.Length != 0)
                            arguments = expression.Arguments;

                        context.WritePolyfillFunction("trim.js", new[] { expression.Object }.Concat(arguments).Append(Expression.Constant("end")));
                    }
                    break;

                case nameof(string.Substring):
                    if (@params.Length == 1)
                        context.WriteMethodCall(expression.Object, "substring", expression.Arguments);
                    else
                        context.WriteMethodCall(
                            expression.Object,
                            "substring",
                            expression.Arguments[0],
                            Expression.MakeBinary(ExpressionType.Add, expression.Arguments[0], expression.Arguments[1])
                        );

                    break;

                case nameof(string.StartsWith):
                    if (@params.Length != 1)
                        throw new NotSupportedException($"{nameof(string.StartsWith)} method with more than one parameters is not supported");
                    context.WriteMethodCall(expression.Object, "startsWith", expression.Arguments);
                    break;

                case nameof(string.EndsWith):
                    if (@params.Length != 1)
                        throw new NotSupportedException($"{nameof(string.EndsWith)} method with more than one parameters is not supported");
                    context.WriteMethodCall(expression.Object, "endsWith", expression.Arguments);
                    break;

                case nameof(string.Remove):
                    if (@params.Length == 1)
                        context.WriteMethodCall(expression.Object, "slice", Expression.Constant(0), expression.Arguments[0]);
                    else
                    {
                        using var addOp = context.Operation(JavascriptOperationTypes.AddSubtract);
                        context.WriteMethodCall(expression.Object, "slice", Expression.Constant(0), expression.Arguments[0]);
                        context.Write('+');
                        var startIndex = Expression.MakeBinary(ExpressionType.Add, expression.Arguments[0], expression.Arguments[1]);
                        context.WriteMethodCall(expression.Object, "slice", startIndex);
                    }
                    break;

                case nameof(string.PadLeft):
                case nameof(string.PadRight):
                    var paddingCharExpr = expression.Arguments.Count == 2 ? expression.Arguments[1] : Expression.Constant(" ");
                    var paddingModeExpr =
                        expression.Method.Name == nameof(string.PadLeft) ? Expression.Constant("left") : Expression.Constant("right");
                    context.WritePolyfillFunction("pad.js", expression.Object, expression.Arguments[0], paddingCharExpr, paddingModeExpr);
                    break;

                case nameof(string.LastIndexOf):
                    if (@params.Length == 1)
                        context.WriteMethodCall(expression.Object, "lastIndexOf", expression.Arguments);
                    else if (@params.Length == 2 && @params[1].ParameterType == typeof(int))
                    {
                        var lengthExpr = Expression.MakeMemberAccess(expression.Object, typeof(string).GetProperty(nameof(string.Length)));
                        var subtractExpr = Expression.MakeBinary(ExpressionType.Subtract, expression.Arguments[1], lengthExpr);
                        var positionExpr = Expression.MakeBinary(ExpressionType.Add, subtractExpr, Expression.Constant(1));
                        context.WriteMethodCall(expression.Object, "lastIndexOf", expression.Arguments[0], positionExpr);
                    }
                    break;
                default:
                    throw new NotSupportedException($"{expression.Method.Name} not supported");
            }
        }
    }
}
