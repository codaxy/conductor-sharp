using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class ListTaskInput : ITaskInput<ListTaskOutput>
    {
        public List<object> List { get; set; }
    }

    public class ListTaskOutput { }

    public class ListTask : SimpleTaskModel<ListTaskInput, ListTaskOutput> { }
}
