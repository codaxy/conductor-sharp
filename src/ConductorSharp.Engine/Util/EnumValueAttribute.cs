using System;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumValueAttribute(object value) : Attribute
    {
        internal object Value { get; } = value;
    }
}
