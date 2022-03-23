using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using XgsPon.Workflows.Client.Model.Common;
using XgsPon.Workflows.Engine.Model;

namespace XgsPon.Workflows.Engine.Model
{
    public class DynamicForkJoinInput : IRequest<NoOutput>
    {
        public dynamic DynamicTasks { get; set; }

        public dynamic DynamicTasksI { get; set; }
    }
    public class DynamicForkJoinTaskModel : TaskModel<DynamicForkJoinInput, NoOutput>
    {
    }
}
