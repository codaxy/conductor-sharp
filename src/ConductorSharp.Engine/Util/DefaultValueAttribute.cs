using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueAttribute : Attribute
    {
        internal object DefaultValue { get; }

        public DefaultValueAttribute(object defaultValue) => DefaultValue = defaultValue;
    }
}
