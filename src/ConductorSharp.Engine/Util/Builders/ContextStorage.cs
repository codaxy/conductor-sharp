using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorSharp.Engine.Util.Builders
{
    public class ContextStorage
    {
        private readonly Dictionary<string, object> _dictStorage = new();

        public object GetOrNulls(string key) => GetOrDefault<object>(key, null);

        public T GetOrDefault<T>(string key, T defaultValue = default) => _dictStorage.TryGetValue(key, out var value) ? (T)value : defaultValue;

        public void Set(string key, object value) => _dictStorage[key] = value;
    }
}
