using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Request
{
    public class WorkflowSearchRequest
    {
        public int? Start { get; set; }
        public int? Size { get; set; }
        public string Sort { get; set; }
        public bool? SortAscending { get; set; }
        public string FreeText { get; set; }
        public string Query { get; set; }
    }
}
