﻿using ConductorSharp.Engine.Builders;
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
                var expressionStrings = methodExpression.Arguments.Skip(1).Select(CompileMemberOrNameExpressions).ToArray();
                var formatExpr = methodExpression.Arguments[0] as ConstantExpression;
                if (formatExpr == null)
                    throw new Exception("string.Format with non constant format string is not supported");
                var formatString = (string)formatExpr.Value;
                return string.Format(formatString, expressionStrings);
            }

            if (expression is NewExpression || expression is MemberInitExpression)
                return ParseObjectInitialization(expression);

            if (expression is NewArrayExpression newArrayExpression)
                return ParseArrayInitalization(newArrayExpression);

            return CompileMemberOrNameExpressions(expression);
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
            else
                return cex.Value;
        }

        private static JArray ParseArrayInitalization(NewArrayExpression newArrayExpression)
        {
            if (newArrayExpression.NodeType != ExpressionType.NewArrayInit)
                throw new Exception("Only dimensionless array initialization is supported");

            return new JArray(newArrayExpression.Expressions.Select(ParseExpression));
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

        private static string CompileMemberOrNameExpressions(Expression expr)
        {
            if (expr is MemberExpression)
                return CreateExpressionString(expr);
            if (
                expr is MethodCallExpression methodExpr
                && methodExpr.Method.DeclaringType == typeof(NamingUtil)
                && methodExpr.Method.Name == nameof(NamingUtil.NameOf)
            )
                return (string)methodExpr.Method.Invoke(null, null);
            throw new Exception($"Expression {expr.GetType().Name} not supported");
        }

        private static string CreateExpressionString(Expression expression)
        {
            var memberList = new List<string>();
            Traverse(expression, memberList);
            memberList.Reverse();
            return $"${{{string.Join('.', memberList)}}}";
        }

        private static void Traverse(Expression expr, List<string> memberList)
        {
            switch (expr)
            {
                case MemberExpression { Member: PropertyInfo propInfo } memEx:

                    if (typeof(WorkflowId).IsAssignableFrom(propInfo.PropertyType))
                    {
                        memberList.AddRange(new[] { "workflowId", "workflow" });
                        ;
                        return;
                    }

                    if (typeof(IWorkflowInput).IsAssignableFrom(propInfo.PropertyType))
                    {
                        memberList.AddRange(new[] { "input", "workflow" });
                        return;
                    }

                    var memberName = GetMemberName(propInfo);
                    memberList.Add(memberName);
                    Traverse(memEx.Expression, memberList);
                    break;

                case UnaryExpression { NodeType: ExpressionType.Convert } unaryEx:
                    Traverse(unaryEx.Operand, memberList);
                    break;

                // Either we reached task property reference (ConstantExpression case) or workflow parameter (ParameterExpression case)
                case ConstantExpression cex when typeof(ITypedWorkflow).IsAssignableFrom(cex.Type):
                case ParameterExpression:
                    break;

                default:
                    throw new NotSupportedException("test");
            }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
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
                memberName = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>(true)?.PropertyName;

            if (memberName == null)
                memberName = SnakeCaseUtil.ToSnakeCase(propertyInfo.Name);

            return memberName;
        }
    }
}
