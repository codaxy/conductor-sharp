using ConductorSharp.Client.Model.Common;
using ConductorSharp.Toolkit.Util;
using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Filters
{
    public class NameWorkflowFilter : IWorkflowFilter
    {
        private readonly Regex[] _regexes;

        public NameWorkflowFilter(IEnumerable<string> names) => _regexes = names.Select(name => RegexUtil.CreateNameRegex(name)).ToArray();

        public bool Test(WorkflowDefinition workflowDefinition) => _regexes.Any(regex => regex.IsMatch(workflowDefinition.Name));
    }
}
