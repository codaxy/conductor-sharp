using System;

namespace ConductorSharp.Client.Util
{

    internal static class StringExtensions
    {
        public static Uri ToRelativeUri(this string pattern, params object[] args)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            return new Uri(string.Format(pattern, args), UriKind.Relative);
        }
    }
}