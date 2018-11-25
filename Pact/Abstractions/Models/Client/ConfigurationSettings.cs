using System.Windows;

namespace Pact.Models.Client
{
    public struct ConfigurationSettings
    {
        private int? _fontSize;
        private string _powerLogFilePath;

        public ConfigurationSettings(
            int cardTextOffset,
            int? fontSize,
            bool hasLoaded,
            string powerLogFilePath,
            Point? trackerWindowLocation,
            Size? trackerWindowSize)
        {
            CardTextOffset = cardTextOffset;
            _fontSize = fontSize;
            HasLoaded = hasLoaded;
            _powerLogFilePath = powerLogFilePath;
            TrackerWindowLocation = trackerWindowLocation;
            TrackerWindowSize = trackerWindowSize;
        }

        public int CardTextOffset { get; set; }

        public int FontSize
        {
            get => _fontSize ?? 12;
            set { _fontSize = value; }
        }

        public bool HasLoaded { get; set; }

        public string PowerLogFilePath
        {
            get => _powerLogFilePath ?? @"C:\Program Files (x86)\Hearthstone\Logs\Power.log";
            set { _powerLogFilePath = value; }
        }

        public Point? TrackerWindowLocation { get; set; }
        public Size? TrackerWindowSize { get; set; }
    }
}
