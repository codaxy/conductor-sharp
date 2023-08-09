using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Client.Model.Response
{
    public class NameAndVersion
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public long CreateTime { get; set; }
    }

    public class WorkflowNamesAndVersionsResponse
    {
        public Dictionary<string, List<NameAndVersion>> Data { get; set; }
    }
}
