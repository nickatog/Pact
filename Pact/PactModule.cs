using System;
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
            string configurationFilePath =
                Path.Combine(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Pact"),
                    ".config");

            builder
            .Register(
                __context =>
                {
                    return new AsyncSemaphore();
                })
            .Named<AsyncSemaphore>("DeckPersistence")
            .SingleInstance();

            builder
            .RegisterType<ConfigurationSettingsViewModel>()
            .AsSelf();

            builder
            .Register(
                __context =>
                    new DeckManagerViewModel(
                        __context.Resolve<ICardInfoProvider>(),
                        __context.Resolve<IDeckImportInterface>(),
                        __context.Resolve<IDeckInfoRepository>(),
                        __context.Resolve<IDecklistSerializer>(),
                        __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                        __context.Resolve<IDeckViewModelFactory>(),
                        __context.Resolve<IEventStream>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("game"),
                        __context.Resolve<ILogger>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<DeckManagerViewModel>();

            // ICardInfoProvider
            builder
            .Register(
                __context =>
                {
                    // Update this to AppData, if we're going to check for file updates on load?
                    string pathToFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if DEBUG
                    pathToFile = Path.Combine(pathToFile, @"..\..\..");
#endif

                    return new JSONCardInfoProvider(Path.Combine(pathToFile, "cards.json"));
                })
            .As<ICardInfoProvider>()
            .SingleInstance();

            // ICollectionSerializers
            builder
            .RegisterGeneric(typeof(VarintCollectionSerializer<>))
            .As(typeof(ICollectionSerializer<>))
            .SingleInstance();

            // IConfigurationSource
            builder
            .Register(
                __context =>
                    new FileBasedConfigurationSource(__context.Resolve<ISerializer<ConfigurationData>>(), configurationFilePath))
            .Named<IConfigurationSource>("base");

            builder
            .RegisterDecorator<IConfigurationSource>(
                (__context, __inner) =>
                    new CachingConfigurationSource(__inner, __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")), "base")
            .SingleInstance();

            // IConfigurationStorage
            builder
            .Register(
                __context =>
                    new FileBasedConfigurationStorage(__context.Resolve<ISerializer<ConfigurationData>>(), configurationFilePath))
            .Named<IConfigurationStorage>("base");

            builder
            .RegisterDecorator<IConfigurationStorage>(
                (__context, __inner) =>
                    new EventDispatchingConfigurationStorage(__inner, __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")), "base")
            .SingleInstance();

            // IDeckImportInterface
            builder
            .RegisterType<DeckImportInterface>()
            .As<IDeckImportInterface>()
            .SingleInstance();

            // IDeckImportModalViewModelFactory
            builder
            .RegisterType<DeckImportViewModelFactory>()
            .As<IDeckImportViewModelFactory>()
            .SingleInstance();

            // IDeckInfoRepository
            builder
            .Register(
                __context =>
                {
                    string filePath =
                        Path.Combine(
                            Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "Pact"),
                            ".decks");

                    return new FileBasedDeckInfoRepository(__context.Resolve<ICollectionSerializer<DeckInfo>>(), filePath);
                })
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

            // IDeckViewModelFactory
            builder
            .Register(
                __context =>
                {
                    return
                        new DeckViewModelFactory(
                            __context.Resolve<ICardInfoProvider>(),
                            __context.Resolve<IDeckImportInterface>(),
                            __context.Resolve<IDeckInfoRepository>(),
                            __context.Resolve<IDecklistSerializer>(),
                            __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                            __context.Resolve<IPlayerDeckTrackerInterface>(),
                            __context.Resolve<Valkyrie.IEventDispatcherFactory>(),
                            __context.ResolveNamed<Valkyrie.IEventDispatcher>("game"),
                            __context.Resolve<ILogger>(),
                            __context.Resolve<IWaitInterface>(),
                            __context.Resolve<IUserConfirmationInterface>(),
                            __context.ResolveNamed<Valkyrie.IEventDispatcher>("view"));
                })
            .As<IDeckViewModelFactory>()
            .SingleInstance();

            // IEventDispatchers
            builder
            .Register(
                __context =>
                    __context.Resolve<Valkyrie.IEventDispatcherFactory>().Create())
            .Named<Valkyrie.IEventDispatcher>("game")
            .SingleInstance();

            builder
            .Register(
                __context =>
                    __context.Resolve<Valkyrie.IEventDispatcherFactory>().Create())
            .Named<Valkyrie.IEventDispatcher>("view")
            .SingleInstance();

            // IEventDispatcherFactory
            builder
            .RegisterType<Valkyrie.InMemoryEventDispatcherFactory>()
            .As<Valkyrie.IEventDispatcherFactory>()
            .SingleInstance();

            // IEventStream
            builder
            .Register(
                __context =>
                    __context.Resolve<IEventStreamFactory>().Create())
            .As<IEventStream>()
            .SingleInstance();

            // IEventStreamFactory
            builder
            .RegisterType<PowerLogEventStreamFactory>()
            .As<IEventStreamFactory>()
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
            .RegisterType<EventParsers.PowerLog.GameStateDebug.PlayerID>()
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

            // IModalDisplay
            builder
            .RegisterType<MainWindowModalDisplay>()
            .As<IModalDisplay>();

            // IPlayerDeckTrackerInterface
            builder
            .RegisterType<WindowedPlayerDeckTrackerInterface>()
            .As<IPlayerDeckTrackerInterface>()
            .SingleInstance();

            // IPlayerDeckTrackerViewModelFactory
            builder
            .Register(
                __context =>
                    new PlayerDeckTrackerViewModelFactory(
                        __context.Resolve<ICardInfoProvider>(),
                        __context.Resolve<IConfigurationSource>(),
                        __context.Resolve<IConfigurationStorage>(),
                        __context.Resolve<ITrackedCardViewModelFactory>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<IPlayerDeckTrackerViewModelFactory>()
            .SingleInstance();
            
            // ISerializers
            builder
            .Register(
                __context =>
                    new Serializer<ConfigurationData>(ConfigurationData.Deserialize))
            .As<ISerializer<ConfigurationData>>()
            .SingleInstance();

            builder
            .Register(
                __context =>
                    new Serializer<DeckInfo>(DeckInfo.Deserialize))
            .As<ISerializer<DeckInfo>>()
            .SingleInstance();

            // ITrackedCardViewModelFactory
            builder
            .Register(
                __context =>
                    new TrackedCardViewModelFactory(
                        __context.Resolve<ICardInfoProvider>(),
                        __context.Resolve<IConfigurationSource>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<ITrackedCardViewModelFactory>()
            .SingleInstance();

            // IUserConfirmation
            builder
            .RegisterType<ModalUserConfirmationInterface>()
            .As<IUserConfirmationInterface>()
            .SingleInstance();

            // IUserConfirmationModalViewModelFactory
            builder
            .RegisterType<UserConfirmationModalViewModelFactory>()
            .As<IUserConfirmationModalViewModelFactory>()
            .SingleInstance();

            // IUserPromptService
            builder
            .RegisterType<UserPrompt>()
            .As<IUserPrompt>()
            .SingleInstance();

            // IWaitInterface
            builder
            .RegisterType<ModalWaitInterface>()
            .As<IWaitInterface>()
            .SingleInstance();

            // IWaitModalViewModelFactory
            builder
            .RegisterType<WaitModalViewModelFactory>()
            .As<IWaitModalViewModelFactory>()
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
    }
}
