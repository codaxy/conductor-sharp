using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    public class BoolInputDescriptorAttribute : InputDescriptorAttribute
    {
        public bool DefaultValue { get; }

        public BoolInputDescriptorAttribute(bool defaultValue, string description, bool required = true) : base(description, required)
        {
            DefaultValue = defaultValue;
        }
    }
}
