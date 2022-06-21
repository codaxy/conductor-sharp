using ConductorSharp.Engine.Util;
using Lambda2Js;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.JsExtensions
{
    internal class MemberInitExpressionExtension : ExpressionExtension<MemberInitExpression>
    {
        public override void HandleExpression(JavascriptConversionContext context, MemberInitExpression expression)
        {
            context.PreventDefault();
            using var op = context.Operation(0);

            if (expression.NewExpression.Arguments.Count != 0)
                throw new NotSupportedException("New expression with constructor arguments not supported");

            context.Write("{");
            bool skipFirstComma = true;

            foreach (var binding in expression.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException($"Only assignment are supported in new expressions");

                var assignment = binding as MemberAssignment;
                if (!skipFirstComma)
                    context.Write(',');
                context.Write(ExpressionUtil.GetMemberName(binding.Member as PropertyInfo));
                context.Write(':');
                context.Write(assignment.Expression);
                skipFirstComma = false;
            }
            context.Write("}");
        }
    }
}
