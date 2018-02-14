namespace Pact
{
    public interface IConfigurationSettings
    {
        string AccountName { get; set; }

        string PowerLogFilePath { get; set; }
    }

    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public string AccountName { get; set; }

        public string PowerLogFilePath { get; set; }
    }
}
