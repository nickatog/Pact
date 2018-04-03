using System.Windows;

namespace Pact
{
    public interface IConfigurationSettings
    {
        int FontSize { get; set; }
        string PowerLogFilePath { get; set; }
        Point? TrackerWindowLocation { get; set; }
        Size? TrackerWindowSize { get; set; }
    }

    public sealed class ConfigurationSettings
        : IConfigurationSettings
    {
        public int FontSize { get; set; }
        public string PowerLogFilePath { get; set; }
        public Point? TrackerWindowLocation { get; set; }
        public Size? TrackerWindowSize { get; set; }
    }
}
