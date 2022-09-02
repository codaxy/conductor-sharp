using ConductorSharp.Client.Model.Common;
using ConductorSharp.Toolkit.Util;
using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Filters
{
    public class NameWorkflowFilter : IWorkflowFilter
    {
        private readonly Regex _regex;

        public NameWorkflowFilter(string name) => _regex = RegexUtil.CreateNameRegex(name);

        public bool Test(WorkflowDefinition workflowDefinition) => _regex.IsMatch(workflowDefinition.Name);
    }
}
