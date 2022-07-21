using Lambda2Js;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConductorSharp.Engine.Util
{
    internal static class JavascriptConversionContextExtensions
    {
        public static JavascriptConversionContext WritePolyfillFunction(
            this JavascriptConversionContext context,
            string functionFile,
            IEnumerable<Expression> arguments
        )
        {
            using var op = context.Operation(JavascriptOperationTypes.Call);
            string function = EmbeddedFileHelper.GetLinesFromEmbeddedFile($"~/Polyfills/{functionFile}");
            context.Write($"({function})");
            context.WriteManyIsolated('(', ')', ',', arguments);
            return context;
        }

        public static JavascriptConversionContext WriteMethodCall(
            this JavascriptConversionContext context,
            Expression obj,
            string method,
            IEnumerable<Expression> arguments
        )
        {
            using var op = context.Operation(JavascriptOperationTypes.Call);
            context.Write(obj);
            context.Write($".{method}");
            context.WriteManyIsolated('(', ')', ',', arguments);
            return context;
        }

        public static JavascriptConversionContext WriteMethodCall(
            this JavascriptConversionContext context,
            Expression obj,
            string method,
            params Expression[] arguments
        ) => context.WriteMethodCall(obj, method, (IEnumerable<Expression>)arguments);
    }
}
