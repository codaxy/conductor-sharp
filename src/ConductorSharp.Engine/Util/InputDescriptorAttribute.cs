using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputDescriptorAttribute : Attribute
    {
        public string Description { get; }
        public bool Required { get; }

        public InputDescriptorAttribute(string description, bool required = true)
        {
            Description = description;
            Required = required;
        }
    }
}
