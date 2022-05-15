using ConductorSharp.Engine.Builders;
using ConductorSharp.Engine.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

        public static JObject ParseToParameters(Expression expression)
        {
            var inputParams = new JObject();

            if (!(expression is MemberInitExpression initExpression))
                throw new Exception($"Only {nameof(MemberInitExpression)} expressions supported");

            return ccex.Value;
        }

        // Handle assignment of value types (boxing)
        if (member.Expression is UnaryExpression unaryEx && unaryEx.NodeType == ExpressionType.Convert)
            return CreateExpressionString(unaryEx);

        // Handle interpolated strings containing references to wf inputs or task outputs
        if (member.Expression is MethodCallExpression methodExpression
                && methodExpression.Method.Name == nameof(string.Format)
                && methodExpression.Method.DeclaringType == typeof(string))
        {
            var expressionStrings = methodExpression.Arguments.Skip(1).Select(expr => CreateExpressionString(expr)).ToArray();
            var formatExpr = methodExpression.Arguments[0] as ConstantExpression;
            if (formatExpr == null)
                throw new Exception("string.Format with non constant format string is not supported");
            var formatString = (string)formatExpr.Value;
            return string.Format(formatString, expressionStrings);
        }

        if (!(member.Expression is MemberExpression))
            throw new Exception($"Only {nameof(MemberExpression)} expressions supported");

        var expression = member.Expression as MemberExpression;

        return CreateExpressionString(expression);
    }

    private static string CreateExpressionString(Expression expression)
    {
        var expressionString = Traverse(expression).Split(".");
        Array.Reverse(expressionString);
        return $"${{{string.Join('.', expressionString)}}}";
    }

    private static string Traverse(Expression expr)
    {
        string memberName = default;

        if (expr is MemberExpression mex)
        {
            var propInfo = mex.Member as PropertyInfo;

            if (typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType))
                memberName = "workflowId.workflow";
            else
                memberName = GetMemberName(propInfo);

            if (mex.Expression is MemberExpression mmex)
            {
                var assignmentValue = ParseToAssignmentString(binding);
                var assignmentKey = GetMemberName(binding.Member as PropertyInfo);

                inputParams.Add(new JProperty(assignmentKey, assignmentValue));
            }

            return inputParams;
        }

        private static object ParseToAssignmentString(MemberBinding mb)
        {
            if (!(mb.BindingType is MemberBindingType.Assignment))
                throw new Exception(
                    $"Only {nameof(MemberBindingType.Assignment)} binding type supported"
                );

            var member = mb as MemberAssignment;

            if (member.Expression is ConstantExpression cex)
                return Convert.ToString(cex.Value);

            if (member.Expression is UnaryExpression uex && uex.Operand is ConstantExpression ccex)
            {
                var converted = Convert.ToString(ccex.Value);

                //if (ccex.Value is bool)
                //    return converted.ToLowerInvariant();

                return ccex.Value;
            }

            if (!(member.Expression is MemberExpression))
                throw new Exception($"Only {nameof(MemberExpression)} expressions supported");

            var expression = member.Expression as MemberExpression;

            var assignmentString = Traverse(expression).Split(".");
            Array.Reverse(assignmentString);
            return $"${{{string.Join('.', assignmentString)}}}";
        }

        private static string Traverse(Expression expr)
        {
            string memberName = default;

            if (expr is MemberExpression mex)
            {
                var propInfo = mex.Member as PropertyInfo;

                if (typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType))
                    memberName = "workflowId.workflow";
                else
                    memberName = GetMemberName(propInfo);

                if (mex.Expression is MemberExpression mmex)
                {
                    var memberType = mmex.Member as PropertyInfo;

                    if (typeof(IWorkflowInput).IsAssignableFrom(memberType.PropertyType))
                        memberName = memberName + "." + "input.workflow";
                    else
                        memberName = memberName + "." + Traverse(mmex);
                }
            }

            return memberName;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }

        private static string GetMemberName(PropertyInfo propertyInfo)
        {
            string memberName = default;

            if (propertyInfo.PropertyType is IParameterKeyword keyword)

                if (memberName == null)
                    memberName = propertyInfo.GetDocSection("originalName");

            if (memberName == null)
                memberName = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>(
                    true
                )?.PropertyName;

            if (memberName == null)
                memberName = SnakeCaseUtil.ToLowercasedPrefixSnakeCase(propertyInfo.Name);

            return memberName;
        }
    }
}