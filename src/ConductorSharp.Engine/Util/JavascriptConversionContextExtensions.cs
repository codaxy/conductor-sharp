using Lambda2Js;

namespace ConductorSharp.Engine.Util
{
    internal static class JavascriptConversionContextExtensions
    {
        public static JavascriptConversionContext WritePolyfillFunction(this JavascriptConversionContext context, string functionFile)
        {
            string function = EmbeddedFileHelper.GetLinesFromEmbeddedFile($"~/Polyfills/{functionFile}");
            context.Write($"({function})");
            return context;
        }
    }
}
