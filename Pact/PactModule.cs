using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
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
            string configurationFilePath =
                Path.Combine(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Pact"),
                    ".config");

            // AsyncSemaphores
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
                        __context.Resolve<IDecklistSerializer>(),
                        __context.Resolve<IDeckRepository>(),
                        __context.Resolve<IEventStreamFactory>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("game"),
                        __context.Resolve<IGameResultRepository>(),
                        __context.Resolve<ILogger>(),
                        __context.Resolve<IPlayerDeckTrackerInterface>(),
                        __context.Resolve<IReplaceDeckInterface>(),
                        __context.Resolve<IUserConfirmationInterface>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
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
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view"));
                })
            .As<ICardDatabaseManager>()
            .SingleInstance();

            // ICardDatabaseUpdateInterface
            builder
            .RegisterType<ModalCardDatabaseUpdateInterface>()
            .As<ICardDatabaseUpdateInterface>()
            .SingleInstance();

            // ICardDatabaseUpdateService
            builder
            .RegisterType<HearthstoneJSONCardDatabaseUpdateService>()
            .As<ICardDatabaseUpdateService>()
            .SingleInstance();

            // ICardInfoProvider
            builder
            .Register(
                __context =>
                    new LocalDatabaseCardInfoProvider(
                        __context.Resolve<ICardDatabase>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
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
                    new FileBasedConfigurationStorage(
                        __context.Resolve<ISerializer<ConfigurationData>>(),
                        configurationFilePath,
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<IConfigurationStorage>()
            .SingleInstance();

            // IDeckImportInterface
            builder
            .RegisterType<DeckImportInterface>()
            .As<IDeckImportInterface>()
            .SingleInstance();

            // IDeckInfoFileStorage
            builder
            .Register(
                __context =>
                    new DeckInfoFileStorage(
                        __context.Resolve<ICollectionSerializer<DeckInfo>>(),
                        Path.Combine(
                            Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "Pact"),
                            ".decks")))
            .As<IDeckInfoFileStorage>()
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

            // IDeckRepository
            builder
            .Register(
                __context =>
                    new FileBasedDeckRepository(
                        __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                        __context.Resolve<IDeckInfoFileStorage>()))
            .As<IDeckRepository>()
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

            // IEventStreamFactory
            builder
            .Register(
                __context =>
                    new PowerLogEventStreamFactory(
                        __context.Resolve<IConfigurationSource>(),
                        __context.Resolve<System.Collections.Generic.IEnumerable<IGameStateDebugEventParser>>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<IEventStreamFactory>()
            .SingleInstance();

            // IGameResultRepository
            builder
            .Register(
                __context =>
                    new FileBasedGameResultRepository(
                        __context.ResolveNamed<AsyncSemaphore>("DeckPersistence"),
                        __context.Resolve<IDeckInfoFileStorage>()))
            .As<IGameResultRepository>()
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
                        __context.Resolve<IEventStreamFactory>(),
                        __context.ResolveNamed<Valkyrie.IEventDispatcher>("view")))
            .As<IPlayerDeckTrackerInterface>()
            .SingleInstance();

            // IReplaceDeckInterface
            builder
            .RegisterType<ModalReplaceDeckInterface>()
            .As<IReplaceDeckInterface>()
            .SingleInstance();

            // ISerializers
            builder
            .Register(
                __context =>
                    new DelegateSerializer<ConfigurationData>(
                        __stream =>
                        {
                            var serializer = new BinaryFormatter();

                            return Task.FromResult((ConfigurationData)serializer.Deserialize(__stream));
                        },
                        (__stream, __item) =>
                        {
                            var serializer = new BinaryFormatter();

                            serializer.Serialize(__stream, __item);

                            return Task.CompletedTask;
                        }))
            .As<ISerializer<ConfigurationData>>()
            .SingleInstance();

            builder
            .Register(
                __context =>
                    new DelegateSerializer<DeckInfo>(
                        __stream =>
                        {
                            var serializer = new BinaryFormatter();

                            return Task.FromResult((DeckInfo)serializer.Deserialize(__stream));
                        },
                        (__stream, __deckInfo) =>
                        {
                            var serializer = new BinaryFormatter();

                            serializer.Serialize(__stream, __deckInfo);

                            return Task.CompletedTask;
                        }))
            .As<ISerializer<DeckInfo>>()
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
