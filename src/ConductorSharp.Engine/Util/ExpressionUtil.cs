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

            if (expression is MemberInitExpression initExpression)
            {
                foreach (var binding in initExpression.Bindings)
                {
                    if (binding.BindingType != MemberBindingType.Assignment)
                        throw new Exception($"Only {nameof(MemberBindingType.Assignment)} binding type supported");

                    var assignmentBinding = (MemberAssignment)binding;
                    var assignmentValue = ParseToAssignmentString(assignmentBinding.Expression);
                    var assignmentKey = GetMemberName(binding.Member as PropertyInfo);

                    inputParams.Add(new JProperty(assignmentKey, assignmentValue));
                }
            }
            // This case handles case when task has empty input parameters (e.g. new() or new TInput())
            // Also this case allows us to handle anonymous types
            else if (expression is NewExpression newExpression
                // With this check we verify it is anonymous type
                && newExpression.Arguments.Count == newExpression.Members.Count)
            {
                foreach (var member in newExpression.Arguments.Zip(newExpression.Members, (expression, memberInfo) => (expression, memberInfo)))
                {
                    var assignmentValue = ParseToAssignmentString(member.expression);
                    var assignmentKey = GetMemberName(member.memberInfo as PropertyInfo);

                    inputParams.Add(new JProperty(assignmentKey, assignmentValue));
                }
            }
            else
                throw new Exception($"Only {nameof(MemberInitExpression)} and {nameof(NewExpression)} without constructor arguments expressions are supported");

            return inputParams;
        }

        private static object ParseToAssignmentString(Expression assignmentExpression)
        {
            if (assignmentExpression is ConstantExpression cex)
                return Convert.ToString(cex.Value);

            if (assignmentExpression is UnaryExpression uex && uex.Operand is ConstantExpression ccex)
            {
                var converted = Convert.ToString(ccex.Value);

                //if (ccex.Value is bool)
                //    return converted.ToLowerInvariant();

                return ccex.Value;
            }

            // Handle boxing
            if (assignmentExpression is UnaryExpression unaryEx && unaryEx.NodeType == ExpressionType.Convert)
                return CreateExpressionString(unaryEx.Operand);
            
            if (assignmentExpression is MethodCallExpression methodExpression 
                && methodExpression.Method.Name == nameof(string.Format) 
                && methodExpression.Method.DeclaringType == typeof(string))
            {
                var expressionStrings = methodExpression.Arguments.Skip(1).Select(CompileMemberOrNameExpressions).ToArray();
                var formatExpr = methodExpression.Arguments[0] as ConstantExpression;
                if (formatExpr == null)
                    throw new Exception("string.Format with non constant format string is not supported");
                var formatString = (string)formatExpr.Value;
                return string.Format(formatString, expressionStrings);
            }

            if (assignmentExpression is NewExpression || assignmentExpression is MemberInitExpression)
                return ParseToParameters(assignmentExpression);

            return CompileMemberOrNameExpressions(assignmentExpression);
        }

        private static string CompileMemberOrNameExpressions(Expression expr)
        {
            if (expr is MemberExpression)
                return CreateExpressionString(expr);
            else if (expr is MethodCallExpression methodExpr
                && methodExpr.Method.DeclaringType == typeof(NamingUtil)
                && methodExpr.Method.Name == nameof(NamingUtil.NameOf))
                return (string)methodExpr.Method.Invoke(null, null);
            else
                throw new Exception($"Expression {expr.GetType().Name} not supported");
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