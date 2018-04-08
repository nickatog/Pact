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
    public sealed class DeckManagerViewModel
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IDeckViewModelFactory _deckViewModelFactory;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly ILogger _logger;

        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        // Should this be a dependency instead? Likely shared outside just this view model and across the entire application
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        public DeckManagerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDeckImportInterface deckImportInterface,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            IDeckViewModelFactory deckViewModelFactory,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            ILogger logger,
            Valkyrie.IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _deckImportInterface = deckImportInterface.Require(nameof(deckImportInterface));
            _deckInfoRepository = deckInfoRepository.Require(nameof(deckInfoRepository));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _deckViewModelFactory = deckViewModelFactory.Require(nameof(deckViewModelFactory));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _logger = logger.Require(nameof(logger));
            _viewEventDispatcher = viewEventDispatcher ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            eventStreamFactory.Require(nameof(eventStreamFactory));

            _gameEventDispatcher = _eventDispatcherFactory.Create();
            RegisterDebugHandlers();

            var eventStream = eventStreamFactory.Create();

            // This event stream should ideally skip all pre-existing events and then begin pumping new events
            // Needs new support added to IEventStream
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
                                (__deck, __sourcePosition) =>
                                {
                                    int position = _decks.IndexOf(__deck);

                                    if (__sourcePosition > _decks.Count)
                                        return;

                                    DeckViewModel sourceDeck = _decks[__sourcePosition];
                                    _decks.RemoveAt(__sourcePosition);

                                    _decks.Insert(position, sourceDeck);

                                    SaveDecks();
                                },
                                __deck => _decks.IndexOf(__deck),
                                __deck =>
                                {
                                    int position = _decks.IndexOf(__deck);

                                    _decks.RemoveAt(position);

                                    SaveDecks();
                                },
                                __deckInfo.DeckID,
                                DeserializeDecklist(__deckInfo.DeckString),
                                __deckInfo.Title,
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
                async () =>
                {
                    // go back to using a view model here? or is it just jumping through hoops at that point to get to an "appropriate" way of doing things
                    // it seems that this would need to know about the view either way
                    // unless this didn't create the view itself, and set a view model property on some top-level object?
                    // then the actual handling of the deck import would happen elsewhere inside the view model itself
                    //var view = new DeckImportView(_decklistSerializer) { Owner = MainWindow.Window };
                    //if (view.ShowDialog() ?? false)

                    DeckImportDetails? deck = await _deckImportInterface.GetDecklist();
                    if (deck == null)
                        return;

                    var deckID = Guid.NewGuid();

                    _decks.Insert(
                        0,
                        _deckViewModelFactory.Create(
                            _gameEventDispatcher,
                            _viewEventDispatcher,
                            (__deck, __sourcePosition) =>
                            {
                                int position = _decks.IndexOf(__deck);

                                if (__sourcePosition > _decks.Count)
                                    return;

                                DeckViewModel sourceDeck = _decks[__sourcePosition];
                                _decks.RemoveAt(__sourcePosition);

                                _decks.Insert(position, sourceDeck);

                                SaveDecks();
                            },
                            __deck => _decks.IndexOf(__deck),
                            __deck =>
                            {
                                int position = _decks.IndexOf(__deck);

                                _decks.RemoveAt(position);

                                SaveDecks();
                            },
                            deckID,
                            deck.Value.Decklist,
                            deck.Value.Title));

                    SaveDecks();
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
                                return new DeckInfo(__deck.DeckID, reader.ReadToEnd(), __deck.Title, (UInt16)_decks.IndexOf(__deck), __deck.GameResults);
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
                    __event => _logger.Write($"{DateTime.Now} - " + (__event.GameWon ? "Won" : "Lost") + $" vs {__event.OpponentHeroCardID}")));

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

        public string GetInterfaces(params string[] names)
        {
            return string.Join(", ", names.Select(__name => "IHas" + __name));
        }
    }

    // Abstract these and move elsewhere
    public struct DeckImportDetails
    {
        public Decklist Decklist { get; private set; }
        public string Title { get; private set; }

        public DeckImportDetails(
            string title,
            Decklist decklist)
        {
            Decklist = decklist;
            Title = title;
        }
    }

    public interface IDeckImportInterface
    {
        Task<DeckImportDetails?> GetDecklist();
    }

    public sealed class DeckImportInterface
        : IDeckImportInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IDeckImportModalViewModelFactory _viewModelFactory;

        public DeckImportInterface(
            IModalDisplay modalDisplay,
            IDeckImportModalViewModelFactory viewModelFactory)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _viewModelFactory = viewModelFactory.Require(nameof(viewModelFactory));
        }

        Task<DeckImportDetails?> IDeckImportInterface.GetDecklist()
        {
            // create view model (via factory?)
            // pass to modal display, along with result handler
            // handler inspects result and creates deck import details object for task completion source

            var result = new TaskCompletionSource<DeckImportDetails?>();

            _modalDisplay.Show(
                _viewModelFactory.Create(),
                __result =>
                {
                    DeckImportDetails? res = null;

                    if (__result.HasValue)
                        res = new DeckImportDetails(__result.Value.Title, __result.Value.Decklist);

                    result.SetResult(res);
                });

            //var view = new DeckImportView(_decklistSerializer) { Owner = MainWindow.Window };
            //if (!(view.ShowDialog() ?? false))
            //    return Task.FromResult<DeckImportDetails?>(default);

            //return Task.FromResult<DeckImportDetails?>(new DeckImportDetails(view.DeckTitle, view.Deck));

            return result.Task;
        }
    }
}
