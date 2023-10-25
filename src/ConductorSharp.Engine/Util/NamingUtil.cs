using ConductorSharp.Engine.Interface;
using System;
using System.Reflection;
using ConductorSharp.Client;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ConductorSharp.Engine.Util
{
    public static class NamingUtil
    {
        internal static string DetermineRegistrationName(Type taskType)
        {
            var originalName = taskType.GetDocSection("originalName");

            if (originalName != null)
                return originalName;

            var originalNameAttribute = (OriginalNameAttribute)Attribute.GetCustomAttribute(taskType, typeof(OriginalNameAttribute));

            if (originalNameAttribute != null)
                return originalNameAttribute.OriginalName;

            var name = SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(taskType.Name);
            //var scope = taskType.Namespace.Split(".").Last().ToUpper();

            //name = name.Replace($"{scope}_", "");

            return name;
            //return $"{scope}_{name}";
        }

        public static string NameOf<TNameable>() where TNameable : INameable => DetermineRegistrationName(typeof(TNameable));

        internal static string GetParameterName(PropertyInfo propInfo) =>
            propInfo.GetDocSection("originalName")
            ?? propInfo.GetCustomAttribute<PropertyNameAttribute>(true)?.Name
            ?? ConductorConstants.IoNamingStrategy.GetPropertyName(propInfo.Name, false);
    }
}
