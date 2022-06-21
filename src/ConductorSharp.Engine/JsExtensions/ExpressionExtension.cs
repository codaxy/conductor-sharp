using Lambda2Js;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.JsExtensions
{
    internal abstract class ExpressionExtension<TExpression> : JavascriptConversionExtension where TExpression : Expression
    {
        public sealed override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (context.Node is TExpression expression)
                HandleExpression(context, expression);
        }

        public abstract void HandleExpression(JavascriptConversionContext context, TExpression expression);

    }
}
