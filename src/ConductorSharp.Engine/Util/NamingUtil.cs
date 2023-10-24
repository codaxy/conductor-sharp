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
            var originalNameAttribute = (OriginalNameAttribute)Attribute.GetCustomAttribute(taskType, typeof(OriginalNameAttribute));

            if (originalNameAttribute != null)
                return originalNameAttribute.OriginalName;

            var name = SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(taskType.Name);
            return name;
        }

        public static string NameOf<TNameable>() where TNameable : INameable => DetermineRegistrationName(typeof(TNameable));

        internal static string GetParameterName(PropertyInfo propInfo) =>
            propInfo.GetCustomAttribute<JsonPropertyAttribute>(true)?.PropertyName
            ?? propInfo.GetCustomAttribute<JsonPropertyNameAttribute>(true)?.Name
            ?? ConductorConstants.IoNamingStrategy.GetPropertyName(propInfo.Name, false);
    }
}
