using System.Windows;

namespace Pact
{
    public struct ConfigurationSettings
    {
        public ConfigurationSettings(
            ConfigurationData configurationData)
        {
            CardTextOffset = configurationData.CardTextOffset;
            FontSize = configurationData.FontSize ?? 12;
            HasLoaded = configurationData.HasLoaded;
            PowerLogFilePath = configurationData.PowerLogFilePath ?? @"C:\Program Files (x86)\Hearthstone\Logs\Power.log";
            TrackerWindowLocation = configurationData.TrackerWindowLocation;
            TrackerWindowSize = configurationData.TrackerWindowSize;
        }

        public int CardTextOffset { get; }

        public int FontSize { get; }

        public bool HasLoaded { get; }

        public string PowerLogFilePath { get; }

        public Point? TrackerWindowLocation { get; }

        public Size? TrackerWindowSize { get; }
    }
}
