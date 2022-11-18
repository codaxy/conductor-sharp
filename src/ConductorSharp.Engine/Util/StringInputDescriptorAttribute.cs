using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    public class StringInputDescriptorAttribute : InputDescriptorAttribute
    {
        public string DefaultValue { get; }

        public StringInputDescriptorAttribute(string defaultValue, string description, bool required = true) : base(description, required)
        {
            DefaultValue = defaultValue;
        }
    }
}
