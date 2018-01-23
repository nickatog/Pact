namespace Pact
{
    public interface IConfigurationSettings
    {
        string AccountName { get; }
    }

    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public string AccountName { get; set; }
    }
}
