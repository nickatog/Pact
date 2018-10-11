using System.Windows;

namespace Pact
{
    public partial class WelcomeView
        : Window
    {
        public WelcomeView(
            WelcomeViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
