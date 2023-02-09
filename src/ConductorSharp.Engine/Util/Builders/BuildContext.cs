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
        public JObject Inputs { get; set; }
        public JObject Outputs { get; set; }
        public WorkflowOptions WorkflowOptions { get; } = new();
        public List<ITaskBuilder> TaskBuilders { get; } = new();
        public ContextStorage Storage { get; } = new();
    }
}
