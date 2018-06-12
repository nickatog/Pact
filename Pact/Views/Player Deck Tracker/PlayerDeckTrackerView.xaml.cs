using System.Windows;

namespace Pact
{
    public partial class PlayerDeckTrackerView
        : Window
    {
        private static PlayerDeckTrackerView _window = new PlayerDeckTrackerView() { Owner = MainWindow.Window };

        public PlayerDeckTrackerView()
        {
            InitializeComponent();
        }

        public static PlayerDeckTrackerView GetWindowFor(
            PlayerDeckTrackerViewModel viewModel)
        {
            if (_window.DataContext is PlayerDeckTrackerViewModel existingViewModel)
                existingViewModel.Cleanup();

            Point? windowLocation = viewModel.ConfigurationSource.GetSettings().Result.TrackerWindowLocation;
            if (windowLocation.HasValue)
            {
                _window.Left = windowLocation.Value.X;
                _window.Top = windowLocation.Value.Y;
            }

            Size? windowSize = viewModel.ConfigurationSource.GetSettings().Result.TrackerWindowSize;
            if (windowSize.HasValue)
            {
                _window.Width = windowSize.Value.Width;
                _window.Height = windowSize.Value.Height;
            }

            _window.DataContext = viewModel;

            return _window;
        }
    }
}
