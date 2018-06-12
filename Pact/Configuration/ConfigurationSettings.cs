using System.Windows;

namespace Pact
{
    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public ConfigurationSettings(
            ConfigurationData configurationData)
        {
            CardTextOffset = configurationData.CardTextOffset;
            FontSize = configurationData.FontSize ?? 12;
            PowerLogFilePath = configurationData.PowerLogFilePath ?? @"C:\Program Files (x86)\Hearthstone\Logs\Power.log";
            TrackerWindowLocation = configurationData.TrackerWindowLocation;
            TrackerWindowSize = configurationData.TrackerWindowSize;
        }

        public int CardTextOffset { get; private set; }

        public int FontSize { get; private set; }

        public string PowerLogFilePath { get; private set; }

        public Point? TrackerWindowLocation { get; private set; }

        public Size? TrackerWindowSize { get; private set; }
    }
}
