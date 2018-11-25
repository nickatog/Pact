namespace Pact
{
    public interface IConfigurationSource
    {
        Models.Client.ConfigurationSettings GetSettings();
    }
}
