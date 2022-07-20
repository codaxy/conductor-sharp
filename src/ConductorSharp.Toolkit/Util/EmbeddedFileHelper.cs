using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace ConductorSharp.Toolkit.Util
{
    internal static class EmbeddedFileHelper
    {
        private static string ReadAssemblyFile(Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);

            if (stream == null)
                throw new InvalidOperationException($"Resource {name} does not exist.");

            using var reader = new StreamReader(stream, Encoding.UTF8);

            return reader.ReadToEnd();
        }

        public static T GetObjectFromEmbeddedFile<T>(string fileName, params (string Key, object Value)[] templateParams)
        {
            fileName = fileName.Replace("~/", typeof(EmbeddedFileHelper).Assembly.GetName().Name + ".").Replace("/", ".");

            var contents = ReadAssemblyFile(typeof(EmbeddedFileHelper).Assembly, fileName);

            if (templateParams != null)
                foreach (var (Key, Value) in templateParams)
                    contents = contents.Replace("{{" + Key + "}}", $"{Value}");

            return JsonConvert.DeserializeObject<T>(contents);
        }

        public static string GetLinesFromEmbeddedFile(string fileName)
        {
            fileName = fileName.Replace("~/", typeof(EmbeddedFileHelper).Assembly.GetName().Name + ".").Replace("/", ".");

            var contents = ReadAssemblyFile(typeof(EmbeddedFileHelper).Assembly, fileName);

            return contents;
        }

        public static Task<T> GetObjectFromEmbeddedFileAsync<T>(string fileName, params (string Key, object Value)[] templateParams) =>
            Task.FromResult(GetObjectFromEmbeddedFile<T>(fileName, templateParams));

        public static string Reserialize<T>(string fileName, params (string Key, object Value)[] templateParams)
        {
            var file = GetObjectFromEmbeddedFile<T>(fileName, templateParams);
            return JsonConvert.SerializeObject(file);
        }
    }
}
