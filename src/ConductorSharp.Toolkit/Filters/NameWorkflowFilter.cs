using System.Text.RegularExpressions;
using ConductorSharp.Client.Generated;
using ConductorSharp.Toolkit.Util;

namespace ConductorSharp.Toolkit.Filters
{
    public class NameWorkflowFilter : IWorkflowFilter
    {
        private readonly Regex[] _regexes;

        public NameWorkflowFilter(IEnumerable<string> names) => _regexes = names.Select(name => RegexUtil.CreateNameRegex(name)).ToArray();

        public bool Test(WorkflowDef workflowDefinition) => _regexes.Any(regex => regex.IsMatch(workflowDefinition.Name));
    }
}
