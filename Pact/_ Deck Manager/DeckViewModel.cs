using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
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
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.ThrowIfNull(nameof(configurationSettings));
            _emplaceDeck = emplaceDeck.ThrowIfNull(nameof(emplaceDeck));
            _eventDispatcherFactory = eventDispatcherFactory.ThrowIfNull(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.ThrowIfNull(nameof(eventStreamFactory));
            _findPosition = findPosition.ThrowIfNull(nameof(findPosition));
            _gameEventDispatcher = gameEventDispatcher.ThrowIfNull(nameof(gameEventDispatcher));
            _gameResultStorage = gameResultStorage.ThrowIfNull(nameof(gameResultStorage));
            _logger = logger.ThrowIfNull(nameof(logger));
            _viewEventDispatcher = viewEventDispatcher.ThrowIfNull(nameof(viewEventDispatcher));

            _delete = delete;
            
            _deckID = deckID;
            _decklist = decklist;
            _title = title;

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

        public string Title => _title ?? string.Empty;

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
                                trackerEventDispatcher,
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

                                bool gameWon = __event.Winners.Contains(_configurationSettings.AccountName);

                                string opponentHeroID = __event.HeroCardIDs.FirstOrDefault(__heroID => __heroID != _decklist.HeroID);
                                opponentHeroID = opponentHeroID ?? _decklist.HeroID;
                                string opponentClass = _cardInfoProvider.GetCardInfo(opponentHeroID)?.Class;

                                var gameResult = new GameResult(timestamp, gameWon, opponentClass);

                                _gameResults.Add(gameResult);

                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Wins"));
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Losses"));

                                _gameResultStorage.SaveGameResult(_deckID, gameResult);
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
                _deckViewModel = deckViewModel.ThrowIfNull(nameof(deckViewModel));
            }

            public DeckViewModel DeckViewModel => _deckViewModel;
        }
    }
}
