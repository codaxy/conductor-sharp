using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Extensions
{
    internal static class FuncExtensions
    {
        public static string GetHash(this Delegate @delegate) => Convert.ToBase64String(@delegate.Method.GetMethodBody().GetILAsByteArray());
    }
}
