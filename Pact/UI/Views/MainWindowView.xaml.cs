using System.Windows;

namespace Pact
{
    public partial class MainWindowView
        : Window
    {
        public static MainWindowView Window { get; private set; }

        public MainWindowView()
        {
            InitializeComponent();

            Window = this;
        }

        public void Initialize(
            MainWindowViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}

// mill rogue: AAECAaIHCLICqAipzQKxzgKA0wLQ4wLf4wK77wILigG0AcQB7QLLA80D+AeGCamvAuXRAtvjAgA=
// zoo: AAECAcn1AgTECJG8ApfTApziAg0w9wSoBc4H5QfCCLy2AsrDApvLAvfNAqbOAvLQAvvTAgA=
// jade druid: AAECAZICBK6rAr6uApS9ApnTAg1AX8QG5Ai0uwLLvALPvALdvgKgzQKHzgKY0gKe0gLb0wIA
