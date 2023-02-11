namespace ConductorSharp.Engine.Util
{
    public class ConfigurationProperty
    {
        public ConfigurationProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public object Value { get; }
    }
}
