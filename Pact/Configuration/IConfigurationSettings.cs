namespace Pact
{
    public interface IConfigurationSettings
    {
        string PowerLogFilePath { get; set; }
    }

    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public string PowerLogFilePath { get; set; }
    }
}
