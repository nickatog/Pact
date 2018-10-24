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

            GlobalConfigurationSource.Instance = _container.Resolve<IConfigurationSource>();
            GlobalConfigurationStorage.Instance = _container.Resolve<IConfigurationStorage>();
        }

        protected override void OnStartup(
            StartupEventArgs e)
        {
            var window = new MainWindowView();

            var hearthstoneConfiguration = _container.Resolve<IHearthstoneConfiguration>();
            hearthstoneConfiguration.EnableLogging();

            window.Initialize(_container.Resolve<MainWindowViewModel>());

            window.Show();
        }
    }
}
