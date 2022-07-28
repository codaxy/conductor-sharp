using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorSharp.Engine.Tests.Util
{
    internal static class SerializationUtil
    {
        public static string SerializeObject(object @object) => JsonConvert.SerializeObject(@object, Formatting.Indented).Replace("\r\n", "\n");
    }
}
