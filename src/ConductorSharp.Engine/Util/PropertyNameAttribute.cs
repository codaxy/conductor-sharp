using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyNameAttribute : Attribute
    {
        internal string Name { get; set; }

        public PropertyNameAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
