using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly Action<DeckViewModel> _delete;
        private readonly Action<DeckViewModel, int> _emplaceDeck;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly Func<DeckViewModel, int> _findPosition;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultStorage _gameResultStorage;
        private readonly ILogger _logger;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        private readonly Guid _deckID;
        private readonly Decklist _decklist;
        private IList<GameResult> _gameResults;
        private string _title;

        public DeckViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDecklistSerializer decklistSerializer,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            IGameResultStorage gameResultStorage,
            ILogger logger,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Action<DeckViewModel, int> emplaceDeck,
            Func<DeckViewModel, int> findPosition,
            Action<DeckViewModel> delete,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults = null)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _emplaceDeck = emplaceDeck.Require(nameof(emplaceDeck));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _findPosition = findPosition.Require(nameof(findPosition));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _gameResultStorage = gameResultStorage.Require(nameof(gameResultStorage));
            _logger = logger.Require(nameof(logger));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _delete = delete;
            
            _deckID = deckID;
            _decklist = decklist;
            _title = title ?? string.Empty;

            Title = _title;

            _gameResults = new List<GameResult>(gameResults ?? Enumerable.Empty<GameResult>());

            _viewEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<DeckTrackingEvent>(
                    __event =>
                    {
                        _canDelete = __event.DeckViewModel != this;

                        _deleteCanExecuteChanged?.Invoke();
                    }));
        }

        private bool _canDelete = true;

        public string Class => _cardInfoProvider.GetCardInfo(_decklist.HeroID)?.Class;

        public ICommand CopyDeck =>
            new DelegateCommand(
                () =>
                {
                    using (var stream = new MemoryStream())
                    {
                        _decklistSerializer.Serialize(stream, _decklist).Wait();

                        stream.Position = 0;

                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);

                        Clipboard.SetText(Encoding.Default.GetString(bytes));
                    }
                });

        public Guid DeckID => _deckID;

        public Decklist Decklist => _decklist;

        private Action _deleteCanExecuteChanged;
        public ICommand Delete =>
            new DelegateCommand(
                () => _delete(this),
                canExecute:
                    () => _canDelete,
                canExecuteChangedClient:
                    __canExecuteChanged => _deleteCanExecuteChanged = __canExecuteChanged);

        public void EmplaceDeck(
            int sourcePosition)
        {
            _emplaceDeck(this, sourcePosition);
        }

        public IEnumerable<GameResult> GameResults => _gameResults;

        public int Losses => _gameResults.Count(__gameResult => !__gameResult.GameWon);

        public int Position => _findPosition(this);

        public ICommand SaveDeckTitle =>
            new DelegateCommand(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")));

        public string Title { get; set; }

        public ICommand TrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _viewEventDispatcher.DispatchEvent(new DeckTrackingEvent(this));

                    Valkyrie.IEventDispatcher trackerEventDispatcher = _eventDispatcherFactory.Create();

                    // This view model also knows about a view... Should it be changed somehow?
                    // We can create the other view model in here, but then what do we do with it?
                    // Require a parameter for an Action<PlayerDeckTrackerViewModel> that will load it?
                    // Feels like that logic will still exist in another view model somewhere though
                    // Route it through an abstraction like IPlayerDeckTracker(.Load(viewModel))?
                    var trackerWindow =
                        PlayerDeckTrackerView.GetWindowFor(
                            new PlayerDeckTrackerViewModel(
                                _cardInfoProvider,
                                _configurationSettings,
                                trackerEventDispatcher,
                                _viewEventDispatcher,
                                _decklist));

                    IEventStream eventStream = _eventStreamFactory.Create();
                    var cancellation = new CancellationTokenSource();

                    Task.Run(
                        async () =>
                        {
                            while (!cancellation.IsCancellationRequested)
                            {
                                try { trackerEventDispatcher.DispatchEvent(await eventStream.ReadNext()); }
                                catch (Exception ex) { await _logger.Write($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"); }
                            }
                        });

                    var recordGameResult =
                        new Valkyrie.DelegateEventHandler<Events.GameEnded>(
                            __event =>
                            {
                                var timestamp = DateTime.UtcNow;

                                string opponentClass = _cardInfoProvider.GetCardInfo(__event.OpponentHeroCardID)?.Class;

                                var gameResult = new GameResult(timestamp, __event.GameWon, opponentClass);

                                _gameResults.Add(gameResult);

                                _gameResultStorage.SaveGameResult(_deckID, gameResult);

                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Wins"));
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Losses"));
                            });

                    _gameEventDispatcher.RegisterHandler(recordGameResult);

                    Valkyrie.IEventHandler unregisterHandlers = null;
                    unregisterHandlers =
                        new Valkyrie.DelegateEventHandler<DeckTrackingEvent>(
                            __event =>
                            {
                                _gameEventDispatcher.UnregisterHandler(recordGameResult);
                                _viewEventDispatcher.UnregisterHandler(unregisterHandlers);

                                cancellation.Cancel();
                            });

                    _viewEventDispatcher.RegisterHandler(unregisterHandlers);

                    trackerWindow.Show();
                });

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);

        public event PropertyChangedEventHandler PropertyChanged;

        private sealed class DeckTrackingEvent
        {
            private readonly DeckViewModel _deckViewModel;

            public DeckTrackingEvent(
                DeckViewModel deckViewModel)
            {
                _deckViewModel = deckViewModel.Require(nameof(deckViewModel));
            }

            public DeckViewModel DeckViewModel => _deckViewModel;
        }
    }
}
