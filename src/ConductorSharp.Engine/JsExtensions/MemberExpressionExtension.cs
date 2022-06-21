using ConductorSharp.Engine.Util;
using Lambda2Js;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.JsExtensions
{
    internal class MemberExpressionExtension : ExpressionExtension<MemberExpression>
    {
        public override void HandleExpression(JavascriptConversionContext context, MemberExpression expression)
        {
            context.PreventDefault();
            context.Write(expression.Expression);
            using var operation = context.Operation(JavascriptOperationTypes.IndexerProperty);
            context.Write(".");
            context.Write(ExpressionUtil.GetMemberName(expression.Member as PropertyInfo));
        }
    }
}
