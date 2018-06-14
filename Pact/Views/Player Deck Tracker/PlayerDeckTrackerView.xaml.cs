using System.Windows;

namespace Pact
{
    public partial class PlayerDeckTrackerView
        : Window
    {
        public PlayerDeckTrackerView(
            PlayerDeckTrackerViewModel viewModel)
        {
            InitializeComponent();

            Owner = MainWindow.Window;

            Point? windowLocation = GlobalConfigurationSource.Instance.GetSettings().TrackerWindowLocation;
            if (windowLocation.HasValue)
            {
                Left = windowLocation.Value.X;
                Top = windowLocation.Value.Y;
            }

            Size? windowSize = GlobalConfigurationSource.Instance.GetSettings().TrackerWindowSize;
            if (windowSize.HasValue)
            {
                Width = windowSize.Value.Width;
                Height = windowSize.Value.Height;
            }

            DataContext = viewModel;
        }
    }
}
