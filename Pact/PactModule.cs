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
        protected override void Load(
            ContainerBuilder builder)
        {
            string appDataDirectoryPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Pact");

            string configurationFilePath = Path.Combine(appDataDirectoryPath, "config.json");

            // AsyncSemaphores
            builder
            .Register(
                __context =>
                    new AsyncSemaphore())
            .Named<AsyncSemaphore>("ConfigurationPersistence")
            .SingleInstance();

            builder
            .Register(
                __context =>
                    new AsyncSemaphore())
            .Named<AsyncSemaphore>("DeckPersistence")
            .SingleInstance();

            // ConfigurationSettingsViewModel
            builder
            .RegisterType<ConfigurationSettingsViewModel>()
            .AsSelf();

            // DeckManagerViewModel
            builder
            .Register(
                __context =>
                    new DeckManagerViewModel(
                        __context.Resolve<IBackgroundWorkInterface>(),
                        __context.Resolve<ICardInfoProvider>(),
                        __context.Resolve<IDeckImportInterface>(),
                        __context.Resolve<ISerializer<Models.Client.Decklist>>(),
                        __context.Resolve<IDeckRepository>(),
                        __context.ResolveNamed<IEventStreamFactory>("Global"),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("Game"),
                        __context.Resolve<IGameResultRepository>(),
                        __context.Resolve<ILogger>(),
                        __context.Resolve<IPlayerDeckTrackerInterface>(),
                        __context.Resolve<IReplaceDeckInterface>(),
                        __context.Resolve<IUserConfirmationInterface>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")))
            .AsSelf();

            // DevToolsViewModel
            builder
            .RegisterType<DevToolsViewModel>()
            .AsSelf();

            // DownloadUpdatesViewModel
            builder
            .RegisterType<DownloadUpdatesViewModel>()
            .AsSelf();

            // IBackgroundWorkInterface
            builder
            .RegisterType<ModalBackgroundWorkInterface>()
            .As<IBackgroundWorkInterface>()
            .SingleInstance();

            // ICardDatabase
            builder
            .Register(
                __context =>
                    new JSONCardDatabase(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "cards.json")))
            .As<ICardDatabase>()
            .SingleInstance();

            // ICardDatabaseManager
            builder
            .Register(
                __context =>
                {
                    string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    return new FileBasedCardDatabaseManager(
                        Path.Combine(appDirectory, "cards.json"),
                        Path.Combine(appDirectory, "cards.version"),
                        appDataDirectoryPath);
                })
            .Named<ICardDatabaseManager>("base");

            builder
            .RegisterDecorator<ICardDatabaseManager>(
                (__context, __inner) =>
                    new EventDispatchingCardDatabaseManager(
                        __inner,
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")),
                "base")
            .SingleInstance();

            // ICardDatabaseUpdateInterface
            builder
            .RegisterType<ModalCardDatabaseUpdateInterface>()
            .As<ICardDatabaseUpdateInterface>()
            .SingleInstance();

            // ICardDatabaseUpdateService
            builder
            .Register(
                __context =>
                    new HearthstoneJSONCardDatabaseUpdateService("https://api.hearthstonejson.com/v1/"))
            .As<ICardDatabaseUpdateService>()
            .SingleInstance();

            // ICardInfoProvider
            builder
            .Register(
                __context =>
                    new LocalDatabaseCardInfoProvider(
                        __context.Resolve<ICardDatabase>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")))
            .As<ICardInfoProvider>()
            .SingleInstance();

            // ICollectionSerializers
            builder
            .RegisterType<JSONCollectionSerializer<Models.Data.Deck>>()
            .As<ICollectionSerializer<Models.Data.Deck>>()
            .SingleInstance();

            builder
            .RegisterType<JSONCollectionSerializer<Models.Data.SavedLog>>()
            .As<ICollectionSerializer<Models.Data.SavedLog>>()
            .SingleInstance();

            // IConfigurationSource
            builder
            .Register(
                __context =>
                    new FileBasedConfigurationSource(
                        __context.Resolve<ISerializer<Models.Data.ConfigurationData>>(),
                        configurationFilePath))
            .Named<IConfigurationSource>("base");

            builder
            .RegisterDecorator<IConfigurationSource>(
                (__context, __inner) =>
                    new CachingConfigurationSource(
                        __inner,
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")),
                "base")
            .SingleInstance();

            // IConfigurationStorage
            builder
            .Register(
                __context =>
                    new FileBasedConfigurationStorage(
                        __context.ResolveNamed<AsyncSemaphore>("ConfigurationPersistence"),
                        __context.Resolve<ISerializer<Models.Data.ConfigurationData>>(),
                        configurationFilePath))
            .Named<IConfigurationStorage>("base");

            builder
            .RegisterDecorator<IConfigurationStorage>(
                (__context, __inner) =>
                    new EventDispatchingConfigurationStorage(
                        __inner,
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")),
                "base")
            .SingleInstance();

            // IDeckImportInterface
            builder
            .RegisterType<ModalDeckImportInterface>()
            .As<IDeckImportInterface>()
            .SingleInstance();

            // IDeckInfoFileStorage
            builder
            .Register(
                __context =>
                    new DeckFileStorage(
                        __context.Resolve<ICollectionSerializer<Models.Data.Deck>>(),
                        Path.Combine(appDataDirectoryPath, "decks.json")))
            .As<IDeckFileStorage>()
            .SingleInstance();

            // IDeckRepository
            builder
            .Register(
                __context =>
                    new FileBasedDeckRepository(
                        __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                        __context.Resolve<IDeckFileStorage>()))
            .As<IDeckRepository>()
            .SingleInstance();

            // IEventDispatchers
            builder
            .Register(
                __context =>
                    __context.Resolve<Valkyrie.IEventDispatcherFactory>().Create())
            .Named<Valkyrie.IEventDispatcher>("Game")
            .SingleInstance();

            builder
            .Register(
                __context =>
                    __context.Resolve<Valkyrie.IEventDispatcherFactory>().Create())
            .Named<Valkyrie.IEventDispatcher>("View")
            .SingleInstance();

            // IEventDispatcherFactory
            builder
            .RegisterType<Valkyrie.InMemoryEventDispatcherFactory>()
            .As<Valkyrie.IEventDispatcherFactory>()
            .SingleInstance();

            // IEventStreamFactories
            builder
            .Register(
                __context =>
                    new PowerLogEventStreamFactory(
                        __context.Resolve<IConfigurationSource>(),
                        __context.Resolve<System.Collections.Generic.IEnumerable<IGameStateDebugEventParser>>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View"),
                        true))
            .Named<IEventStreamFactory>("Global")
            .SingleInstance();

            builder
            .Register(
                __context =>
                    new PowerLogEventStreamFactory(
                        __context.Resolve<IConfigurationSource>(),
                        __context.Resolve<System.Collections.Generic.IEnumerable<IGameStateDebugEventParser>>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View"),
                        false))
            .Named<IEventStreamFactory>("Instance")
            .SingleInstance();

            // IGameResultRepository
            builder
            .Register(
                __context =>
                    new FileBasedGameResultRepository(
                        __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                        __context.Resolve<IDeckFileStorage>()))
            .As<IGameResultRepository>()
            .SingleInstance();

            // IGameStateDebugEventParsers
            builder
            .RegisterType<GameStateDebugEventParsers.Block>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.CreateGame>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.FullEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.HideEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.PlayerID>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.ShowEntity>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            builder
            .RegisterType<GameStateDebugEventParsers.TagChange>()
            .As<IGameStateDebugEventParser>()
            .SingleInstance();

            // IHearthstoneConfiguration
            builder
            .RegisterType<HearthstoneConfiguration>()
            .As<IHearthstoneConfiguration>()
            .SingleInstance();

            // Add compiler directive to only use debug logger in DEBUG, otherwise use file logger
            // ILogger
            builder
            .RegisterType<DebugLogger>()
            .As<ILogger>()
            .SingleInstance();

            // ILogManagementInterface
            builder
            .Register(
                __context =>
                    new ModalLogManagementInterface(
                        __context.Resolve<IModalDisplay>(),
                        __context.Resolve<IPowerLogManager>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")))
            .As<ILogManagementInterface>()
            .SingleInstance();

            // IModalDisplay
            builder
            .RegisterType<MainWindowModalDisplay>()
            .As<IModalDisplay>()
            .SingleInstance();

            // IPlayerDeckTrackerInterface
            builder
            .Register(
                __context =>
                    new WindowedPlayerDeckTrackerInterface(
                        __context.Resolve<ICardInfoProvider>(),
                        __context.Resolve<IConfigurationSource>(),
                        __context.Resolve<Valkyrie.IEventDispatcherFactory>(),
                        __context.ResolveNamed<IEventStreamFactory>("Instance"),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")))
            .As<IPlayerDeckTrackerInterface>()
            .SingleInstance();

            // IPowerLogManager
            builder
            .Register(
                __context =>
                    new FileBasedPowerLogManager(
                        __context.Resolve<IConfigurationSource>(),
                        Path.Combine(appDataDirectoryPath, "SavedLogs"),
                        "logs.json",
                        __context.Resolve<ICollectionSerializer<Models.Data.SavedLog>>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("View")))
            .As<IPowerLogManager>()
            .SingleInstance();

            // IReplaceDeckInterface
            builder
            .RegisterType<ModalReplaceDeckInterface>()
            .As<IReplaceDeckInterface>()
            .SingleInstance();

            // ISerializers
            builder
            .RegisterType<JSONSerializer<Models.Data.ConfigurationData>>()
            .As<ISerializer<Models.Data.ConfigurationData>>()
            .SingleInstance();

            builder
            .RegisterType<VarintDecklistSerializer>()
            .Named<ISerializer<Models.Client.Decklist>>("base");

            builder
            .RegisterDecorator<ISerializer<Models.Client.Decklist>>(
                (__context, __inner) =>
                    new TextDecklistSerializer(__inner), "base")
            .SingleInstance();

            // IUserConfirmationInterface
            builder
            .RegisterType<ModalUserConfirmationInterface>()
            .As<IUserConfirmationInterface>()
            .SingleInstance();

            // IUserPrompt
            builder
            .RegisterType<UserPrompt>()
            .As<IUserPrompt>()
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
