#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class DeckViewModelFactory
        : IDeckViewModelFactory
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly AsyncSemaphore _deckPersistenceMutex;
        private readonly IPlayerDeckTrackerInterface _deckTrackerInterface;
        private readonly IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly ILogger _logger;
        private readonly IBackgroundWorkInterface _notifyWaiter;
        private readonly Dispatcher _uiThreadDispatcher;
        private readonly IUserConfirmationInterface _userConfirmation;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public DeckViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            AsyncSemaphore deckPersistenceMutex,
            IPlayerDeckTrackerInterface deckTrackerInterface,
            IEventDispatcherFactory eventDispatcherFactory,
            IEventDispatcher gameEventDispatcher,
            ILogger logger,
            IBackgroundWorkInterface notifyWaiter,
            Dispatcher uiThreadDispatcher,
            IUserConfirmationInterface userConfirmation,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _deckImportInterface =
                deckImportInterface
                ?? throw new ArgumentNullException(nameof(deckImportInterface));

            _deckInfoRepository =
                deckInfoRepository
                ?? throw new ArgumentNullException(nameof(deckInfoRepository));

            _decklistSerializer =
                decklistSerializer
                ?? throw new ArgumentNullException(nameof(decklistSerializer));

            _deckPersistenceMutex =
                deckPersistenceMutex
                ?? throw new ArgumentNullException(nameof(deckPersistenceMutex));
            _deckTrackerInterface =
                deckTrackerInterface
                ?? throw new ArgumentNullException(nameof(deckTrackerInterface));

            _eventDispatcherFactory =
                eventDispatcherFactory
                ?? throw new ArgumentNullException(nameof(eventDispatcherFactory));

            _gameEventDispatcher =
                gameEventDispatcher
                ?? throw new ArgumentNullException(nameof(gameEventDispatcher));

            _logger =
                logger
                ?? throw new ArgumentNullException(nameof(logger));

            _notifyWaiter =
                notifyWaiter
                ?? throw new ArgumentNullException(nameof(notifyWaiter));

            _uiThreadDispatcher =
                uiThreadDispatcher
                ?? throw new ArgumentNullException(nameof(uiThreadDispatcher));

            _userConfirmation =
                userConfirmation
                ?? throw new ArgumentNullException(nameof(userConfirmation));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));
        }
        #endregion // Constructors

        DeckViewModel IDeckViewModelFactory.Create(
            Func<DeckViewModel, int> findPosition,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults)
        {
            return
                new DeckViewModel(
                    _cardInfoProvider,
                    _deckImportInterface,
                    _deckInfoRepository,
                    _decklistSerializer,
                    _deckPersistenceMutex,
                    _deckTrackerInterface,
                    _eventDispatcherFactory,
                    _gameEventDispatcher,
                    _logger,
                    _notifyWaiter,
                    _uiThreadDispatcher,
                    _userConfirmation,
                    _viewEventDispatcher,
                    findPosition,
                    deckID,
                    decklist,
                    title,
                    gameResults);
        }
    }
}
