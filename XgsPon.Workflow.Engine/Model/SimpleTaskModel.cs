using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Engine.Interface;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Model
{
    public abstract class SimpleTaskModel<I, O> : TaskModel<I, O> where I : IRequest<O>
    {
    }
}
