using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;

namespace Pact
{
    public sealed class PactModule
        : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // ICardInfoProvider
            builder
            .Register(
                __context =>
                {
                    string pathToFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
                    pathToFile = Path.Combine(pathToFile, @"..\..\..");
#endif

                    return new JSONCardInfoProvider(Path.Combine(pathToFile, "cards.json"));
                })
            .As<ICardInfoProvider>()
            .SingleInstance();

            // IConfigurationSettings
            builder
#if DEBUG
            .RegisterType<HardCodedConfigurationSettings>()
#else
            .RegisterType<ConfigurationSettings>()
#endif
            .As<IConfigurationSettings>()
            .SingleInstance();

            // IDeckInfoRepository
            builder
            .RegisterType<DeckInfoRepository>()
            .As<IDeckInfoRepository>()
            .SingleInstance();

            // IDecklistSerializer
            builder
            .RegisterType<VarintDecklistSerializer>()
            .Named<IDecklistSerializer>("base");

            builder
            .RegisterDecorator<IDecklistSerializer>(
                (__context, __inner) =>
                    new TextDecklistSerializer(__inner), "base")
            .SingleInstance();

            // DeckViewModelFactory
            builder
            .RegisterType<DeckViewModelFactory>()
            .As<IDeckViewModelFactory>()
            .SingleInstance();

            // IEventDispatcherFactory
            builder
            .RegisterType<Valkyrie.InMemoryEventDispatcherFactory>()
            .As<Valkyrie.IEventDispatcherFactory>()
            .SingleInstance();

            // IEventStreamFactory
            builder
            .RegisterType<PowerLogEventStreamFactory>()
            .As<IEventStreamFactory>()
            .SingleInstance();

            // IGameResultStorage
            builder
            .RegisterType<GameResultStorage>()
            .As<IGameResultStorage>()
            .SingleInstance();

            // IGameStateDebugEventParsers
            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.Block>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.CreateGame>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.FullEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.HideEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.ShowEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<EventParsers.PowerLog.GameStateDebug.TagChange>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            // Add compiler directive to only use debug logger in DEBUG, otherwise use file logger
            // ILogger
            builder
            .RegisterType<DebugLogger>()
            .As<ILogger>()
            .SingleInstance();

            // MainWindowViewModel
            builder
            .RegisterType<MainWindowViewModel>()
            .AsSelf();
        }

        private sealed class DebugLogger
            : ILogger
        {
            Task ILogger.Write(string message)
            {
                System.Diagnostics.Debug.WriteLine(message);

                return Task.CompletedTask;
            }
        }

        private sealed class HardCodedConfigurationSettings
            : IConfigurationSettings
        {
            public HardCodedConfigurationSettings()
            {
                AccountName = "Nickatog";
                PowerLogFilePath = @"C:\Program Files (x86)\Hearthstone\Logs\Power.log";
            }

            public string AccountName { get; set; }
            public string PowerLogFilePath { get; set; }
        }
    }
}
