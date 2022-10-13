using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Model
{
    public class DynamicTaskInput<I, O> : IRequest<O>
    {
        public I TaskInput { get; set; }
        public string TaskToExecute { get; set; }
    }

    public class DynamicTaskModel<I, O> : TaskModel<DynamicTaskInput<I, O>, O> { }
}
