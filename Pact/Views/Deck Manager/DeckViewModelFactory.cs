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
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IGameResultStorage _gameResultStorage;
        private readonly ILogger _logger;

        public DeckViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDecklistSerializer decklistSerializer,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            IGameResultStorage gameResultStorage,
            ILogger logger)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _gameResultStorage = gameResultStorage.Require(nameof(gameResultStorage));
            _logger = logger.Require(nameof(logger));
        }

        DeckViewModel IDeckViewModelFactory.Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Action<DeckViewModel, int> emplaceDeck,
            Func<DeckViewModel, int> findPosition,
            Action<DeckViewModel> delete,
            Action<DeckViewModel> saveDeck,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults)
        {
            return
                new DeckViewModel(
                    _cardInfoProvider,
                    _configurationSettings,
                    _decklistSerializer,
                    _eventDispatcherFactory,
                    _eventStreamFactory,
                    gameEventDispatcher,
                    _gameResultStorage,
                    _logger,
                    viewEventDispatcher,
                    emplaceDeck,
                    findPosition,
                    delete,
                    saveDeck,
                    deckID,
                    decklist,
                    title,
                    gameResults);
        }
    }
}
