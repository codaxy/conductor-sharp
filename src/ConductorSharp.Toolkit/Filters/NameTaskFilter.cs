using ConductorSharp.Client.Generated;
using ConductorSharp.Toolkit.Util;
using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Filters
{
    public class NameTaskFilter : ITaskFilter
    {
        private readonly Regex[] _regexes;

        public NameTaskFilter(IEnumerable<string> names) => _regexes = names.Select(name => RegexUtil.CreateNameRegex(name)).ToArray();

        public bool Test(TaskDef taskDefinition) => _regexes.Any(regex => regex.IsMatch(taskDefinition.Name));
    }
}
