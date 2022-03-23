using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;

namespace XgsPon.Workflows.Engine.Interface
{
    public interface ITypedWorkflow
    {
        WorkflowDefinition GetDefinition();
    }
}
