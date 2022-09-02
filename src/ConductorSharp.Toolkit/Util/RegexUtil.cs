using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Util
{
    public static class RegexUtil
    {
        public static Regex CreateNameRegex(string name) => new($"^{Regex.Escape(name).Replace("%", ".*")}$", RegexOptions.Compiled);
    }
}
