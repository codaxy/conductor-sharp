using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Exceptions;
using ConductorSharp.Engine.Interface;
using Newtonsoft.Json.Linq;

namespace ConductorSharp.Engine.Util
{
    public static class ExpressionUtil
    {
        public static string ParseToReferenceName(Expression expression)
        {
            if (expression is not MemberExpression taskSelectExpression)
                throw new NotSupportedException($"Only {nameof(MemberExpression)} expression allowed");

            return SnakeCaseUtil.ToSnakeCase(taskSelectExpression.Member.Name);
        }

        public static Type ParseToType(Expression expression)
        {
            if (expression is not MemberExpression taskSelectExpression)
                throw new NotSupportedException($"Only {nameof(MemberExpression)} expression allowed");

            return ((PropertyInfo)taskSelectExpression.Member).PropertyType;
        }

        public static JObject ParseToParameters(Expression expression) => ParseObjectInitialization(expression);

        private static object ParseExpression(Expression expression)
        {
            if (expression is ConstantExpression cex)
                return ParseConstantExpression(cex);

            // Handle boxing
            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryEx)
                return ParseExpression(unaryEx.Operand);

            if (expression is BinaryExpression binaryEx)
                return ParseBinaryExpression(binaryEx);

            if (IsStringInterpolation(expression))
                return CompileStringInterpolationExpression((MethodCallExpression)expression);

            if (expression is NewExpression || expression is MemberInitExpression)
                return ParseObjectInitialization(expression);

            if (expression is NewArrayExpression newArrayExpression)
                return ParseArrayInitalization(newArrayExpression);

            if (IsListInitExpression(expression))
                return ParseListInit((ListInitExpression)expression);

            if (IsDictionaryInitialization(expression))
                return ParseDictionaryInitialization((ListInitExpression)expression);

            if (ShouldCompileToJsonPathExpression(expression))
                return CreateExpressionString(expression);

            if (IsNameOfExpression(expression))
                return CompileNameOfExpression((MethodCallExpression)expression);

            return EvaluateExpression(expression);
        }

        private static bool IsStringInterpolation(Expression expression) =>
            expression is MethodCallExpression { Method.Name: nameof(string.Format) } methodExpression
            && methodExpression.Method.DeclaringType == typeof(string);

        private static object EvaluateExpression(Expression expr) =>
            IsEvaluatable(expr) ? Expression.Lambda(expr).Compile().DynamicInvoke() : throw new NonEvaluatableExpressionException(expr);

        private static bool IsEvaluatable(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression memExpr:
                    if (
                        typeof(ITaskModel).IsAssignableFrom(memExpr.Type)
                        || typeof(WorkflowId).IsAssignableFrom(memExpr.Type)
                        || typeof(IWorkflowInput).IsAssignableFrom(memExpr.Type)
                    )
                        return false;
                    return IsEvaluatable(memExpr.Expression);
                case MethodCallExpression methodExpr:
                    return IsEvaluatable(methodExpr.Object) && methodExpr.Arguments.All(IsEvaluatable);
                case BinaryExpression binaryExpr:
                    return IsEvaluatable(binaryExpr.Left) && IsEvaluatable(binaryExpr.Right);
                case UnaryExpression unaryExpr:
                    return IsEvaluatable(unaryExpr.Operand);
                case ConditionalExpression condExpr:
                    return IsEvaluatable(condExpr.Test) && IsEvaluatable(condExpr.IfTrue) && IsEvaluatable(condExpr.IfFalse);
                default:
                    return true;
            }
        }

        private static object CompileStringInterpolationExpression(MethodCallExpression methodExpression)
        {
            var interpolationArguments = GetInterpolationArguments(methodExpression);
            var expressionStrings = interpolationArguments.Select(CompileInterpolatedStringArgument).ToArray();
            var formatExpr =
                methodExpression.Arguments[0] as ConstantExpression
                ?? throw new NotSupportedException("string.Format with non constant format string is not supported");
            var formatString = (string)formatExpr.Value;
            return string.Format(formatString, expressionStrings);
        }

        private static IEnumerable<Expression> GetInterpolationArguments(MethodCallExpression methodExpression)
        {
            var methodParams = methodExpression.Method.GetParameters();

            //If it is Format(String, Object[]) overload then extract arguments from array, otherwise we assume it is 1, 2, or 3 formatting argument method
            return methodParams.Length == 2 && methodParams[0].ParameterType == typeof(string) && methodParams[1].ParameterType.IsArray
                ? ((NewArrayExpression)methodExpression.Arguments[1]).Expressions
                : methodExpression.Arguments.Skip(1); // Skip format string
        }

        // We want to be able to specify input like this
        // Input = wf.WorkflowInput.IpAddress + "/24"
        // Hence why we should not evaluate binary expressions
        private static object ParseBinaryExpression(BinaryExpression binaryEx)
        {
            var left = ParseExpression(binaryEx.Left);
            var right = ParseExpression(binaryEx.Right);

            // Use these lambdas to extract MemberExpression for operands
            Expression<Func<object>> leftExpr = () => left;
            Expression<Func<object>> rightExpr = () => right;

            // Compiler generates following expression tree for primitve + string concatenation operation
            // (object)(primitive) + string
            // Notice the casting to object, primitive is boxed before concatenation


            // If either of operands is string and the other one is primitive that means we are performing primitive + string concatenation
            // We should not box the primitive in this case (since it is object already)
            // In this case we should cast it to concrete type, otherwise binary operation will fail
            var lhs =
                left.GetType().IsPrimitive && right.GetType() == typeof(string) ? leftExpr.Body : Expression.Convert(leftExpr.Body, left.GetType());
            var rhs =
                right.GetType().IsPrimitive && left.GetType() == typeof(string)
                    ? rightExpr.Body
                    : Expression.Convert(rightExpr.Body, right.GetType());
            var expr = binaryEx.Update(lhs, null, rhs);

            return EvaluateExpression(expr);
        }

        private static object ParseConstantExpression(ConstantExpression cex)
        {
            var type = cex.Type;
            if (type.IsEnum)
            {
                // Handle enum constants
                var field = type.GetField(type.GetEnumName(cex.Value));
                return field.GetCustomAttribute<EnumValueAttribute>()?.Value ?? cex.Value.ToString();
            }

            return cex.Value;
        }

        private static bool IsDictionaryIndexExpression(Expression expr) =>
            expr is MethodCallExpression mex
            && mex.Method.Name == "get_Item"
            && mex.Method.DeclaringType != null
            && mex.Method.DeclaringType.IsGenericType
            && mex.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>);

        private static JArray ParseArrayInitalization(NewArrayExpression newArrayExpression)
        {
            if (newArrayExpression.NodeType != ExpressionType.NewArrayInit)
                throw new Exception("Only dimensionless array initialization is supported");

            return new JArray(newArrayExpression.Expressions.Select(ParseExpression));
        }

        private static bool IsListInitExpression(Expression expr) =>
            expr is ListInitExpression listExpr && listExpr.Initializers.All(i => i.Arguments.Count == 1);

        private static JArray ParseListInit(ListInitExpression listInitExpression)
        {
            var array = new JArray();
            foreach (var init in listInitExpression.Initializers)
            {
                array.Add(ParseExpression(init.Arguments.Single()));
            }
            return array;
        }

        private static bool IsDictionaryInitialization(Expression expr) =>
            expr is ListInitExpression listExpr && listExpr.NewExpression.Type.IsAssignableTo(typeof(IDictionary<string, object>));

        private static JObject ParseDictionaryInitialization(ListInitExpression listExpression)
        {
            var obj = new JObject();

            foreach (var init in listExpression.Initializers)
            {
                obj.Add((((ConstantExpression)init.Arguments[0]).Value as string)!, JToken.FromObject(ParseExpression(init.Arguments[1])));
            }

            return obj;
        }

        private static JObject ParseObjectInitialization(Expression expression)
        {
            var inputParams = new JObject();

            if (expression is MemberInitExpression initExpression)
            {
                foreach (var binding in initExpression.Bindings)
                {
                    if (binding.BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException($"Only {nameof(MemberBindingType.Assignment)} binding type supported");

                    var assignmentBinding = (MemberAssignment)binding;
                    var assignmentValue = ParseExpression(assignmentBinding.Expression);
                    var assignmentKey = GetMemberName(binding.Member as PropertyInfo);

                    inputParams.Add(new JProperty(assignmentKey, assignmentValue));
                }
            }
            // This case handles case when task has empty input parameters (e.g. new() or new TInput())
            // Also this case allows us to handle anonymous types
            else if (
                expression is NewExpression newExpression
                // With this check we verify it is anonymous type
                && newExpression.Arguments.Count == (newExpression.Members?.Count ?? 0)
            )
            {
                foreach (
                    var member in newExpression.Arguments.Zip(
                        newExpression.Members ?? (IEnumerable<MemberInfo>)[],
                        (expression, memberInfo) => (expression, memberInfo)
                    )
                )
                {
                    var assignmentValue = ParseExpression(member.expression);
                    var assignmentKey = GetMemberName(member.memberInfo as PropertyInfo);

                    inputParams.Add(new JProperty(assignmentKey, assignmentValue));
                }
            }
            else
                throw new NotSupportedException(
                    $"Only {nameof(MemberInitExpression)} and {nameof(NewExpression)} without constructor arguments expressions are supported"
                );

            return inputParams;
        }

        private static object CompileInterpolatedStringArgument(Expression expr)
        {
            if (ShouldCompileToJsonPathExpression(expr))
                return CreateExpressionString(expr);
            if (IsNameOfExpression(expr))
                return CompileNameOfExpression((MethodCallExpression)expr);
            if (expr is ConstantExpression cex)
                return ParseConstantExpression(cex);
            if (expr is UnaryExpression { NodeType: ExpressionType.Convert } uex)
                return CompileInterpolatedStringArgument(uex.Operand);
            return EvaluateExpression(expr);
        }

        private static bool IsNameOfExpression(Expression expr) =>
            expr is MethodCallExpression { Method.Name: nameof(NamingUtil.NameOf) } methodExpr
            && methodExpr.Method.DeclaringType == typeof(NamingUtil);

        private static string CompileNameOfExpression(MethodCallExpression methodExpr) => (string)methodExpr.Method.Invoke(null, null);

        private static string CreateExpressionString(Expression expression)
        {
            return $"${{{CompileToJsonPathExpression(expression)}}}";
        }

        private static string CompileToJsonPathExpression(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression { Member: PropertyInfo propInfo } memEx:

                    if (typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType))
                    {
                        return "workflow.workflowId";
                    }

                    if (typeof(IWorkflowInput).IsAssignableFrom(propInfo.PropertyType))
                    {
                        return "workflow.input";
                    }

                    var memberName = GetMemberName(propInfo);

                    return IsTerminalPropertyExpression(memEx.Expression)
                        ? memberName
                        : $"{CompileToJsonPathExpression(memEx.Expression)}.{memberName}";

                case MethodCallExpression mex when IsDictionaryIndexExpression(mex):
                    return $"{CompileToJsonPathExpression(mex.Object)}[{CompileToJsonPathExpression(mex.Arguments[0])}]";

                case UnaryExpression { NodeType: ExpressionType.Convert } unaryEx:
                    return CompileToJsonPathExpression(unaryEx.Operand);

                case ConstantExpression constantExpr:
                    return CompileJsonPathConstant(constantExpr);

                default:
                    throw new NotSupportedException($"Expression {expr} not supported while traversing members");
            }
        }

        private static bool ShouldCompileToJsonPathExpression(Expression expr)
        {
            if (expr is not MemberExpression && !IsDictionaryIndexExpression(expr))
                return false;

            return CheckIfRootExpressionIsTaskModel(expr);
        }

        // Either we reached task property reference (ConstantExpression case) or workflow parameter (ParameterExpression case)
        private static bool IsTerminalPropertyExpression(Expression expr) =>
            (expr is ConstantExpression || expr is ParameterExpression) && typeof(ITypedWorkflow).IsAssignableFrom(expr.Type);

        private static bool CheckIfRootExpressionIsTaskModel(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression { Member: PropertyInfo propInfo } memEx:

                    if (
                        typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType)
                        || typeof(IWorkflowInput).IsAssignableFrom(propInfo.PropertyType)
                        || (typeof(ITaskModel).IsAssignableFrom(propInfo.PropertyType) && IsTerminalPropertyExpression(memEx.Expression))
                    )
                        return true;

                    return CheckIfRootExpressionIsTaskModel(memEx.Expression);

                case MethodCallExpression mex when IsDictionaryIndexExpression(mex):
                    return CheckIfRootExpressionIsTaskModel(mex.Object);

                case UnaryExpression { NodeType: ExpressionType.Convert } unaryEx:
                    return CheckIfRootExpressionIsTaskModel(unaryEx.Operand);

                default:
                    return false;
            }
        }

        private static string CompileJsonPathConstant(ConstantExpression cex)
        {
            if (cex.Type == typeof(string))
                return $"'{cex.Value}'";
            return cex.Value.ToString();
        }

        private static string GetMemberName(PropertyInfo propertyInfo) => NamingUtil.GetParameterName(propertyInfo);
    }
}
