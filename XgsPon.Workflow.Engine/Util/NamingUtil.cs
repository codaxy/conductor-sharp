using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XgsPon.Workflows.Engine.Util
{
    public static class NamingUtil
    {
        public static string DetermineRegistrationName(Type taskType)
        {
            var originalName = taskType.GetDocSection("originalName");

            if (originalName != null)
                return originalName;

            var originalNameAttribute = (OriginalNameAttribute)Attribute.GetCustomAttribute(
                taskType,
                typeof(OriginalNameAttribute)
            );

            if (originalNameAttribute != null)
                return originalNameAttribute.OriginalName;

            var name = SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(taskType.Name);
            //var scope = taskType.Namespace.Split(".").Last().ToUpper();

            //name = name.Replace($"{scope}_", "");

            return name;
            //return $"{scope}_{name}";
        }

        public static string DetermineReferenceName(string referenceName) =>
            SnakeCaseUtil.ToCapitalizedPrefixSnakeCase(referenceName);
    }
}
