namespace ConductorSharp.Engine.Util
{
    public class ConfigurationProperty(string key, object value)
    {
        public string Key { get; } = key;
        public object Value { get; } = value;
    }
}
