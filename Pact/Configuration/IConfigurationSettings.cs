namespace Pact
{
    public interface IConfigurationSettings
    {
        int FontSize { get; set; }
        string PowerLogFilePath { get; set; }
    }

    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public int FontSize { get; set; }
        public string PowerLogFilePath { get; set; }
    }
}
