using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class MainWindowViewModel
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IDeckViewModelFactory _deckViewModelFactory;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly ILogger _logger;

        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;
        
        public MainWindowViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            IDeckViewModelFactory deckViewModelFactory,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            ILogger logger)
        {
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.ThrowIfNull(nameof(configurationSettings));
            _deckInfoRepository = deckInfoRepository.ThrowIfNull(nameof(deckInfoRepository));
            _decklistSerializer = decklistSerializer.ThrowIfNull(nameof(decklistSerializer));
            _deckViewModelFactory = deckViewModelFactory.ThrowIfNull(nameof(deckViewModelFactory));
            _eventDispatcherFactory = eventDispatcherFactory.ThrowIfNull(nameof(eventDispatcherFactory));
            _logger = logger.ThrowIfNull(nameof(logger));

            eventStreamFactory.ThrowIfNull(nameof(eventStreamFactory));

            _viewEventDispatcher = _eventDispatcherFactory.Create();

            _gameEventDispatcher = _eventDispatcherFactory.Create();
            RegisterDebugHandlers();

            var eventStream = eventStreamFactory.Create();

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        try { _gameEventDispatcher.DispatchEvent(await eventStream.ReadNext()); }
                        catch (Exception ex) { await _logger.Write($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"); }
                    }
                });

            _decks =
                new ObservableCollection<DeckViewModel>(
                    _deckInfoRepository.GetAll().Result
                    .OrderBy(__deckInfo => __deckInfo.Position)
                    .Select(
                        __deckInfo =>
                            _deckViewModelFactory.Create(
                                _gameEventDispatcher,
                                _viewEventDispatcher,
                                __deck =>
                                {
                                    int position = _decks.IndexOf(__deck);
                                    if (position <= 0)
                                        return;

                                    _decks.RemoveAt(position);

                                    _decks.Insert(position - 1, __deck);

                                    SaveDecks();
                                },
                                __deck =>
                                {
                                    int position = _decks.IndexOf(__deck);
                                    if (position >= _decks.Count - 1)
                                        return;

                                    _decks.RemoveAt(position);

                                    _decks.Insert(position + 1, __deck);

                                    SaveDecks();
                                },
                                __deck =>
                                {
                                    // Will this cause any problems if a deck that is currently being tracked is deleted?

                                    int position = _decks.IndexOf(__deck);

                                    _decks.RemoveAt(position);

                                    SaveDecks();
                                },
                                __deckInfo.DeckID,
                                DeserializeDecklist(__deckInfo.DeckString),
                                __deckInfo.GameResults)));

            Decklist DeserializeDecklist(string text)
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(text)))
                    return _decklistSerializer.Deserialize(stream).Result;
            }
        }

        private readonly IList<DeckViewModel> _decks;
        public IEnumerable<DeckViewModel> Decks => _decks;

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    // go back to using a view model here? or is it just jumping through hoops at that point to get to an "appropriate" way of doing things
                    // it seems that this would need to know about the view either way
                    // unless this didn't create the view itself, and set a view model property on some top-level object?
                    // then the actual handling of the deck import would happen elsewhere inside the view model itself
                    var view = new DeckImportView(_decklistSerializer) { Owner = MainWindow.Window };
                    if (view.ShowDialog() ?? false)
                    {
                        var deckID = Guid.NewGuid();

                        _decks.Add(
                            _deckViewModelFactory.Create(
                                _gameEventDispatcher,
                                _viewEventDispatcher,
                                __deck =>
                                {
                                    int position = _decks.IndexOf(__deck);
                                    if (position <= 0)
                                        return;

                                    _decks.RemoveAt(position);

                                    _decks.Insert(position - 1, __deck);

                                    SaveDecks();
                                },
                                __deck =>
                                {
                                    int position = _decks.IndexOf(__deck);
                                    if (position >= _decks.Count - 1)
                                        return;

                                    _decks.RemoveAt(position);

                                    _decks.Insert(position + 1, __deck);

                                    SaveDecks();
                                },
                                __deck =>
                                {
                                    // Will this cause any problems if a deck that is currently being tracked is deleted?

                                    int position = _decks.IndexOf(__deck);

                                    _decks.RemoveAt(position);

                                    SaveDecks();
                                },
                                deckID,
                                view.Deck));

                        SaveDecks();
                    }
                });

        private void SaveDecks()
        {
            var deckInfos =
                _decks.Select(
                    __deck =>
                    {
                        using (var stream = new MemoryStream())
                        {
                            _decklistSerializer.Serialize(stream, __deck.Decklist);

                            stream.Position = 0;

                            using (var reader = new StreamReader(stream))
                                return new DeckInfo(__deck.DeckID, reader.ReadToEnd(), (UInt16)_decks.IndexOf(__deck), __deck.GameResults);
                        }
                    });

            _deckInfoRepository.Save(deckInfos).Wait();
        }

        [Conditional("DEBUG")]
        private void RegisterDebugHandlers()
        {
            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event => _logger.Write($"{DateTime.Now} - Player card draw: {_cardInfoProvider.GetCardInfo(__event.CardID).Value.Name}")));

            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardEnteredPlayFromDeck>(
                    __event => _logger.Write($"{DateTime.Now} - Card entered play from deck: {_cardInfoProvider.GetCardInfo(__event.CardID).Value.Name}")));

            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.GameEnded>(
                    __event => _logger.Write($"{DateTime.Now} - Game ended: {string.Join(", ", __event.Winners)} beat {string.Join(", ", __event.Losers)}!")));

            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentCoinLost>(
                    __event => _logger.Write($"{DateTime.Now} - Opponent no longer has the coin!")));

            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __event => _logger.Write($"{DateTime.Now} - Opponent received the coin!")));

            _gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.PlayerReceivedCoin>(
                    __event => _logger.Write($"{DateTime.Now} - Player received the coin!")));
        }
    }
}
