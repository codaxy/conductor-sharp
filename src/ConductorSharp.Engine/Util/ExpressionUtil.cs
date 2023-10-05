using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Extensions;
using ConductorSharp.Engine.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConductorSharp.Engine.Util
{
    public static class ExpressionUtil
    {
        public static string ParseToReferenceName(Expression expression)
        {
            if (!(expression is MemberExpression taskSelectExpression))
                throw new Exception($"Only {nameof(MemberExpression)} expression allowed");

            return SnakeCaseUtil.ToSnakeCase(taskSelectExpression.Member.Name);
        }

        public static Type ParseToType(Expression expression)
        {
            if (!(expression is MemberExpression taskSelectExpression))
                throw new Exception($"Only {nameof(MemberExpression)} expression allowed");

            return ((PropertyInfo)taskSelectExpression.Member).PropertyType;
        }

        public static JObject ParseToParameters(Expression expression) => ParseObjectInitialization(expression);

        private static object ParseExpression(Expression expression)
        {
            if (expression is ConstantExpression cex)
                return ParseConstantExpression(cex);

            // Handle boxing
            if (expression is UnaryExpression unaryEx && unaryEx.NodeType == ExpressionType.Convert)
                return ParseExpression(unaryEx.Operand);

            if (expression is BinaryExpression binaryEx)
                return ParseBinaryExpression(binaryEx);

            if (
                expression is MethodCallExpression methodExpression
                && methodExpression.Method.Name == nameof(string.Format)
                && methodExpression.Method.DeclaringType == typeof(string)
            )
            {
                return CompileStringInterpolationExpression(methodExpression);
            }

            if (expression is NewExpression || expression is MemberInitExpression)
                return ParseObjectInitialization(expression);

            if (expression is NewArrayExpression newArrayExpression)
                return ParseArrayInitalization(newArrayExpression);

            if (expression is ListInitExpression listInitExpression)
                return ParseListInit(listInitExpression);

            if (ShouldCompileToJsonPathExpression(expression))
                return CreateExpressionString(expression);

            if (IsNameOfExpression(expression))
                return CompileNameOfExpression((MethodCallExpression)expression);

            throw new NotSupportedException($"Expression {expression} not supported in current context");
        }

        //private static bool Is

        private static object CompileStringInterpolationExpression(MethodCallExpression methodExpression)
        {
            var interpolationArguments = GetInterpolationArguments(methodExpression);
            var expressionStrings = interpolationArguments.Select(CompileInterpolatedStringArgument).ToArray();
            var formatExpr = methodExpression.Arguments[0] as ConstantExpression;
            if (formatExpr == null)
                throw new Exception("string.Format with non constant format string is not supported");
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

        private static object ParseBinaryExpression(BinaryExpression binaryEx)
        {
            var left = ParseExpression(binaryEx.Left);
            var right = ParseExpression(binaryEx.Right);

            switch (binaryEx.NodeType)
            {
                case ExpressionType.Add:

                    if (left is string leftStr)
                        return leftStr + right;
                    if (right is string rightStr)
                        return left + rightStr;

                    throw new NotSupportedException($"Expression {left} + {right} not supported");
                default:
                    throw new NotSupportedException($"Binary expression with node type {binaryEx.NodeType} not supported");
            }
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

        private static JArray ParseListInit(ListInitExpression listInitExpression)
        {
            var array = new JArray();
            foreach (ElementInit init in listInitExpression.Initializers)
            {
                // Assuming all initializers have one argument
                if (init.Arguments.Count != 1)
                {
                    throw new NotSupportedException("Only single argument initializers are supported");
                }

                array.Add(ParseExpression(init.Arguments.Single()));
            }
            return array;
        }

        private static JObject ParseObjectInitialization(Expression expression)
        {
            var inputParams = new JObject();

            if (expression is MemberInitExpression initExpression)
            {
                foreach (var binding in initExpression.Bindings)
                {
                    if (binding.BindingType != MemberBindingType.Assignment)
                        throw new Exception($"Only {nameof(MemberBindingType.Assignment)} binding type supported");

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
                        newExpression.Members ?? (IEnumerable<MemberInfo>)new List<MemberInfo>(),
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
                throw new Exception(
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
            if (expr is UnaryExpression uex && uex.NodeType == ExpressionType.Convert)
                return CompileInterpolatedStringArgument(uex.Operand);

            throw new NotSupportedException($"Expression {expr.GetType().Name} in interpolated string not supported");
        }

        private static bool IsNameOfExpression(Expression expr) =>
            expr is MethodCallExpression methodExpr
            && methodExpr.Method.DeclaringType == typeof(NamingUtil)
            && methodExpr.Method.Name == nameof(NamingUtil.NameOf);

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

                    // Either we reached task property reference (ConstantExpression case) or workflow parameter (ParameterExpression case)
                    // case ConstantExpression cex when typeof(ITypedWorkflow).IsAssignableFrom(cex.Type)
                    if (
                        (memEx.Expression is ConstantExpression cex && typeof(ITypedWorkflow).IsAssignableFrom(cex.Type))
                        || memEx.Expression is ParameterExpression
                    )
                        return memberName;
                    return $"{CompileToJsonPathExpression(memEx.Expression)}.{memberName}";

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

        private static bool CheckIfRootExpressionIsTaskModel(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression { Member: PropertyInfo propInfo } memEx:

                    if (
                        typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType)
                        || typeof(IWorkflowInput).IsAssignableFrom(propInfo.PropertyType)
                        || typeof(ITaskModel).IsAssignableFrom(propInfo.PropertyType)
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
