using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;

namespace XgsPon.Workflows.Engine.Interface
{
    public interface ITaskBuilder
    {
        //void Build();
        WorkflowDefinition.Task[] Build();
    }
}
