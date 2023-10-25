using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ConductorSharp.Engine.Util;

namespace ConductorSharp.Engine.Model
{
    public class WaitTaskInput : IRequest<NoOutput>
    {
        [PropertyName("duration")]
        public string Duration { get; set; }

        [PropertyName("until")]
        public string Until { get; set; }
    }

    public class WaitTaskModel : TaskModel<WaitTaskInput, NoOutput> { }
}
