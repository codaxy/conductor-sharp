using System.Text.RegularExpressions;

namespace ConductorSharp.Toolkit.Util
{
    internal static class LangUtils
    {
        // There are more characters, so far this is enough
        private const string InvalidCharactersRegex = @" |@|,|:|\||;|\||\(|\)|\[|\]|\.";

        public static bool MakeValidMemberName(string memberName, string prefix, out string validMemberName)
        {
            bool changedMemberName = false;
            validMemberName = memberName;

            if (char.IsDigit(memberName[0]))
            {
                validMemberName = $"{prefix}{memberName}";
                changedMemberName = true;
            }

            if (RemoveInvalidCharacters(validMemberName, out validMemberName))
                changedMemberName = true;

            return changedMemberName;
        }

        private static bool RemoveInvalidCharacters(string memberName, out string validMemberName)
        {
            validMemberName = Regex.Replace(memberName, InvalidCharactersRegex, string.Empty);
            return memberName != validMemberName;
        }
    }
}
