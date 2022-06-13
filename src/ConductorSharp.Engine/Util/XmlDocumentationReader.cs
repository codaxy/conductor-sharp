using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ConductorSharp.Engine.Util
{

    public static class XmlDocumentationReader
    {
        private static string GetDirectoryPath(this Assembly assembly)
        {
            //var codeBase = assembly.Location;
            //var uri = new UriBuilder(codeBase);
            //var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(assembly.Location);
        }

        private static HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();
        public static void LoadXmlDocumentation(Assembly assembly)
        {
            if (loadedAssemblies.Contains(assembly))
                return;

            var directoryPath = assembly.GetDirectoryPath();
            var xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
            if (File.Exists(xmlFilePath))
            {
                LoadXmlDocumentation(File.ReadAllText(xmlFilePath));
                loadedAssemblies.Add(assembly);
            }
        }

        private static Dictionary<string, XElement> loadedXmlDocumentation = new Dictionary<
            string,
            XElement
        >();
        private static void LoadXmlDocumentation(string xmlDocumentation)
        {
            var doc = XDocument.Parse(xmlDocumentation);
            foreach (var element in doc.Element("doc").Element("members").Elements())
            {
                var xmlName = element.Attribute("name").Value;
                loadedXmlDocumentation.Add(xmlName, element);
            }
        }
        private static string XmlDocumentationKeyHelper(
            string typeFullNameString,
            string memberNameString
        )
        {
            var key = Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty).Replace('+', '.');
            if (memberNameString != null)
                key += "." + memberNameString;

            return key;
        }

        public static string GetDocSection(this Type type, string name)
        {
            var key = "T:" + XmlDocumentationKeyHelper(type.FullName, null);
            return GetByKey(key, name);
        }

        public static string GetDocSection(this PropertyInfo propertyInfo, string name)
        {
            var key =
                "P:"
                + XmlDocumentationKeyHelper(propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            return GetByKey(key, name);
        }

        private static string GetByKey(string key, string name)
        {
            loadedXmlDocumentation.TryGetValue(key, out var documentation);
            var ownerEmail = documentation?.Element(name)?.Value?.Trim();

            return !string.IsNullOrEmpty(ownerEmail)
                ? string.Join('\n', ownerEmail.Split("\n").Select(a => a.TrimStart()))
                : null;
        }
    }
}