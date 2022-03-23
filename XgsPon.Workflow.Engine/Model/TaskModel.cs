using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Model
{
    public abstract class TaskModel<I, O> where I : IRequest<O>
    {
        public I Input { get; set; }
        public O Output { get; set; }
    }
}
