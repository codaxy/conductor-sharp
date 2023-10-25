using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConductorSharp.Engine.Util
{
    internal class IOContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            // This property instantation is copied from base.CreateProperty source code, only addition is DeterminePropertyName
            // This also means that other Newtonsoft.Json property attributes (like JsonIgnore) are ignored
            var property = new JsonProperty
            {
                PropertyType = ((PropertyInfo)member).PropertyType,
                DeclaringType = member.DeclaringType,
                ValueProvider = CreateMemberValueProvider(member),
                AttributeProvider = new ReflectionAttributeProvider(member),
                UnderlyingName = member.Name,
                PropertyName = DeterminePropertyName(member),
                Readable = true,
                Writable = true,
            };

            return property;
        }

        private string DeterminePropertyName(MemberInfo member)
        {
            var attributeMemberName = member.GetCustomAttribute<PropertyNameAttribute>()?.Name;
            var nameToProcess = attributeMemberName ?? member.Name;
            return NamingStrategy?.GetPropertyName(nameToProcess, attributeMemberName != null) ?? nameToProcess;
        }
    }
}
