using Lambda2Js;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.JsExtensions
{
    internal class ParameterExpressionExtension : ExpressionExtension<ParameterExpression>
    {
        public override void HandleExpression(JavascriptConversionContext context, ParameterExpression expression)
        {
            context.PreventDefault();
            using var op = context.Operation(0);
            context.Write('$');
        }
    }
}
