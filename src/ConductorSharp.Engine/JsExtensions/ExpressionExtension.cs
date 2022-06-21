using Lambda2Js;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.JsExtensions
{
    internal abstract class ExpressionExtension<TExpression> : JavascriptConversionExtension where TExpression : Expression
    {
        public sealed override void ConvertToJavascript(JavascriptConversionContext context)
        {
            if (context.Node is not TExpression)
                return;

            HandleExpression(context, (TExpression)context.Node);
        }

        public abstract void HandleExpression(JavascriptConversionContext context, TExpression expression);

    }
}
