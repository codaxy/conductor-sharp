using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util
{
    public class OptionsInputDescriptorAttribute : InputDescriptorAttribute
    {
        public string[] Options { get; }
        public string DefaultValue { get; set; }

        public OptionsInputDescriptorAttribute(string description, string[] options, bool required = true) : base(description, required)
        {
            Options = options;
            DefaultValue = options?[0] ?? "";
            Options = options;
        }
    }
}
