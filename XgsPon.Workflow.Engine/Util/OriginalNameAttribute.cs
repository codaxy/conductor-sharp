using System;
using System.Collections.Generic;
using System.Text;

namespace XgsPon.Workflows.Engine.Util
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OriginalNameAttribute : Attribute
    {
        public string OriginalName { get; }
        public OriginalNameAttribute(string originalName) => OriginalName = originalName;
    }
}
