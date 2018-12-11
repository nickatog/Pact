using System.Windows;

namespace Pact.Models.Data
{
    public struct ConfigurationData
    {
        public int CardTextOffset;
        public int? FontSize;
        public bool HasLoaded;
        public string PowerLogFilePath;
        public string TextEditorFilePath;
        public Point? TrackerWindowLocation;
        public Size? TrackerWindowSize;

        public ConfigurationData(
            int cardTextOffset,
            int? fontSize,
            bool hasLoaded,
            string powerLogFilePath,
            string textEditorFilePath,
            Point? trackerWindowLocation,
            Size? trackerWindowSize)
        {
            CardTextOffset = cardTextOffset;
            FontSize = fontSize;
            HasLoaded = hasLoaded;
            PowerLogFilePath = powerLogFilePath;
            TextEditorFilePath = textEditorFilePath;
            TrackerWindowLocation = trackerWindowLocation;
            TrackerWindowSize = trackerWindowSize;
        }
    }
}
