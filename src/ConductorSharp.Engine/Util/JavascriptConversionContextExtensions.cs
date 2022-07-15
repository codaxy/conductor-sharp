using Lambda2Js;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Util
{
    internal static class JavascriptConversionContextExtensions
    {
        public static JavascriptConversionContext WritePolyfillFunction(this JavascriptConversionContext context, string functionFile, IEnumerable<Expression> arguments)
        {
            string function = EmbeddedFileHelper.GetLinesFromEmbeddedFile($"~/Polyfills/{functionFile}");
            context.Write($"({function})");
            context.WriteManyIsolated('(', ')', ',', arguments);
            return context;
        }
    }
}
