using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class ArrayTaskInput : IRequest<ArrayTaskOutput>
    {
        public class TestModel
        {
            public string String { get; set; }
        }

        public int[] Integers { get; set; }
        public TestModel[] Models { get; set; }
        public object Objects { get; set; }
    }

    public class ArrayTaskOutput { }

    public class ArrayTask : SimpleTaskModel<ArrayTaskInput, ArrayTaskOutput> { }
}
