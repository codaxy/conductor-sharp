using System;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Exceptions
{
    public class NonEvaluatableExpressionException : InvalidOperationException
    {
        public Expression Expression { get; }

        public NonEvaluatableExpressionException(Expression expression)
            : base($"{expression} can not be evauluated, it references task input/output or wf input")
        {
            Expression = expression;
        }
    }
}
