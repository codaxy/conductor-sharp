namespace ConductorSharp.Engine.Tests.Util
{
    public interface IConfigurationService
    {
        T GetValue<T>(string key);
    }

    public class ConfigurationService : IConfigurationService
    {
        public T GetValue<T>(string key) => default;
    }
}
