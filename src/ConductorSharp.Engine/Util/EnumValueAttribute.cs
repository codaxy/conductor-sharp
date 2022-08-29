using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueAttribute : Attribute
    {
        internal object Value { get; }

        public EnumValueAttribute(object value) => Value = value;
    }
}
