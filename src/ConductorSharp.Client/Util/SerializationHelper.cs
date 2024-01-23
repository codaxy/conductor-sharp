using Newtonsoft.Json;

namespace ConductorSharp.Client.Util
{
    public static class SerializationHelper
    {
        public static T? DictonaryToObject<T>(IDictionary<string, object> dict, JsonSerializerSettings serializerSettings)
        {
            var json = JsonConvert.SerializeObject(dict, serializerSettings);
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        public static object? DictonaryToObject(Type objectType, IDictionary<string, object> dict, JsonSerializerSettings serializerSettings)
        {
            var json = JsonConvert.SerializeObject(dict, serializerSettings);
            return JsonConvert.DeserializeObject(json, objectType, serializerSettings);
        }

        public static IDictionary<string, object>? ObjectToDictionary(object obj, JsonSerializerSettings serializerSettings)
        {
            var json = JsonConvert.SerializeObject(obj, serializerSettings);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(json, serializerSettings);
        }
    }
}
