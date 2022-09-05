using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Samples.Tasks
{
    public class VersionSubworkflowInput : IRequest<VersionSubworkflowOutput> { }

    public class VersionSubworkflowOutput { }

    [OriginalName("TEST_subworkflow")]
    [Version(3)]
    public class VersionSubworkflow : SubWorkflowTaskModel<VersionSubworkflowInput, VersionSubworkflowOutput> { }
}
