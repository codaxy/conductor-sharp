using ConductorSharp.Client.Model.Common;
using ConductorSharp.Toolkit.Util;
using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Filters
{
    public class NameTaskFilter : ITaskFilter
    {
        private readonly Regex _regex;

        public NameTaskFilter(string name) => _regex = RegexUtil.CreateNameRegex(name);

        public bool Test(TaskDefinition taskDefinition) => _regex.IsMatch(taskDefinition.Name);
    }
}
