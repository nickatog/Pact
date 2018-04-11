using System.Windows;

namespace Pact
{
    public interface IConfigurationSettings
    {
        int CardTextOffset { get; set; }
        int FontSize { get; set; }
        string PowerLogFilePath { get; set; }
        Point? TrackerWindowLocation { get; set; }
        Size? TrackerWindowSize { get; set; }
    }
}
