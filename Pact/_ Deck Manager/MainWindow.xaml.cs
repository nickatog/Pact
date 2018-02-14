using System.Windows;
using Autofac;

namespace Pact
{
    public partial class MainWindow
        : Window
    {
        public static MainWindow Window { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new PactModule());

            IContainer container = builder.Build();

            DataContext = container.Resolve<MainWindowViewModel>();

            Window = this;
        }
    }
}

// mill rogue: AAECAaIHCLICqAipzQKxzgKA0wLQ4wLf4wK77wILigG0AcQB7QLLA80D+AeGCamvAuXRAtvjAgA=
// zoo: AAECAcn1AgTECJG8ApfTApziAg0w9wSoBc4H5QfCCLy2AsrDApvLAvfNAqbOAvLQAvvTAgA=
// jade druid: AAECAZICBK6rAr6uApS9ApnTAg1AX8QG5Ai0uwLLvALPvALdvgKgzQKHzgKY0gKe0gLb0wIA
