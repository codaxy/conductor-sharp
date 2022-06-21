using Lambda2Js;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConductorSharp.Engine.JsExtensions
{
    internal abstract class TypeMethodsExtension<TType> : ExpressionExtension<MethodCallExpression>
    {
        public override void HandleExpression(JavascriptConversionContext context, MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType != typeof(TType))
                return;

            HandleMethodExpression(context, expression);
        }

        public abstract void HandleMethodExpression(JavascriptConversionContext context, MethodCallExpression expression);
    }
}
