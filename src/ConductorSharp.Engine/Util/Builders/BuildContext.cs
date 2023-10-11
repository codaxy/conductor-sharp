using ConductorSharp.Engine.Interface;
using ConductorSharp.Engine.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util.Builders
{
    public class BuildContext
    {
        public List<string> Inputs { get; set; }
        public JObject Outputs { get; set; }
        public WorkflowOptions WorkflowOptions { get; } = new();
        public string WorkflowName { get; set; }
    }
}
