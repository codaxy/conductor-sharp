using ConductorSharp.Engine.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsTypedProperty(this PropertyInfo propertyInfo) => propertyInfo.GetCustomAttribute<TypedPropertyAttribute>() != null;

        public static PropertyInfo GetUntypedProperty(this PropertyInfo propertyInfo)
        {
            if (!propertyInfo.IsTypedProperty())
                throw new InvalidOperationException($"Property {propertyInfo.Name} is untyped");

            return propertyInfo.DeclaringType.GetProperty(propertyInfo.GetCustomAttribute<TypedPropertyAttribute>().PropertyName);
        }
    }
}
