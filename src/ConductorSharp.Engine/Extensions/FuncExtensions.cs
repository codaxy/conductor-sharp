using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    internal static class FuncExtensions
    {
        private static readonly SHA1 _sha1 = SHA1.Create();

        public static string GetHash(this Delegate @delegate) =>
            Convert.ToBase64String(_sha1.ComputeHash(@delegate.Method.GetMethodBody().GetILAsByteArray())).Replace('+', '-').Replace('/', '_');
    }
}
