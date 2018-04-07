using System.Windows;
using Autofac;

namespace Pact
{
    public partial class App
        : Application
    {
        private readonly IContainer _container;

        private App()
        {
            InitializeComponent();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new PactModule());

            builder.RegisterInstance(Dispatcher);

            _container = builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var window = new MainWindow();

            //if (_container.Resolve<IUserPrompt>().Display("This is a test!", "OK", "Cancel"))
            //    ;

            window.Initialize(_container.Resolve<MainWindowViewModel>());

            window.Show();
        }
    }
}
