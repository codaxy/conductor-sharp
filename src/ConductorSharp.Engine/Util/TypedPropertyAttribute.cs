using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TypedPropertyAttribute : Attribute
    {
        public TypedPropertyAttribute(string propertyName) => PropertyName = propertyName;

        public string PropertyName { get; }
    }
}
