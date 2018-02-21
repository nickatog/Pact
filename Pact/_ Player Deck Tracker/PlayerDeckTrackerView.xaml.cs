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

            _window.DataContext = viewModel;

            return _window;
        }
    }
}
