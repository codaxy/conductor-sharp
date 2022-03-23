using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Model
{
    public abstract class LambdaOutputModel<O>
    {
        public O Result { get; set; }
    }
    public abstract class LambdaTaskModel<I, O> where I : IRequest<O>
    {
        public I Input { get; set; }
        public LambdaOutputModel<O> Output { get; set; }
    }
}
