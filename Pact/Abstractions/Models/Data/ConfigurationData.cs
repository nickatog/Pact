using System.Windows;

namespace Pact.Models.Data
{
    public struct ConfigurationData
    {
        public int CardTextOffset;
        public int? FontSize;
        public bool HasLoaded;
        public string PowerLogFilePath;
        public Point? TrackerWindowLocation;
        public Size? TrackerWindowSize;

        public ConfigurationData(
            int cardTextOffset,
            int? fontSize,
            bool hasLoaded,
            string powerLogFilePath,
            Point? trackerWindowLocation,
            Size? trackerWindowSize)
        {
            CardTextOffset = cardTextOffset;
            FontSize = fontSize;
            HasLoaded = hasLoaded;
            PowerLogFilePath = powerLogFilePath;
            TrackerWindowLocation = trackerWindowLocation;
            TrackerWindowSize = trackerWindowSize;
        }
    }
}
