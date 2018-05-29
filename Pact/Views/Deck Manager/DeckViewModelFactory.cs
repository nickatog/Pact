using System;
using System.Collections.Generic;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckViewModelFactory
        : IDeckViewModelFactory
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly AsyncSemaphore _deckPersistenceMutex;
        private readonly IDeckTrackerInterface _deckTrackerInterface;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly ILogger _logger;
        private readonly IWaitInterface _notifyWaiter;
        private readonly IUserConfirmationInterface _userConfirmation;

        public DeckViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDeckImportInterface deckImportInterface,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            AsyncSemaphore deckPersistenceMutex,
            IDeckTrackerInterface deckTrackerInterface,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            ILogger logger,
            IWaitInterface notifyWaiter,
            IUserConfirmationInterface userConfirmation)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _deckImportInterface = deckImportInterface.Require(nameof(deckImportInterface));
            _deckInfoRepository = deckInfoRepository.Require(nameof(deckInfoRepository));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _deckPersistenceMutex = deckPersistenceMutex.Require(nameof(deckPersistenceMutex));
            _deckTrackerInterface = deckTrackerInterface.Require(nameof(deckTrackerInterface));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _logger = logger.Require(nameof(logger));
            _notifyWaiter = notifyWaiter.Require(nameof(notifyWaiter));
            _userConfirmation = userConfirmation.Require(nameof(userConfirmation));
        }

        DeckViewModel IDeckViewModelFactory.Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Func<DeckViewModel, int> findPosition,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults)
        {
            return
                new DeckViewModel(
                    _cardInfoProvider,
                    _configurationSettings,
                    _deckImportInterface,
                    _deckInfoRepository,
                    _decklistSerializer,
                    _deckPersistenceMutex,
                    _deckTrackerInterface,
                    _eventDispatcherFactory,
                    _eventStreamFactory,
                    gameEventDispatcher,
                    _logger,
                    _notifyWaiter,
                    _userConfirmation,
                    viewEventDispatcher,
                    findPosition,
                    deckID,
                    decklist,
                    title,
                    gameResults);
        }
    }
}
