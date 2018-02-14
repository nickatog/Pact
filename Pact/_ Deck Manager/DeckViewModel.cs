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
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultStorage _gameResultStorage;
        private readonly ILogger _logger;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;
        
        private readonly Guid _deckID;
        private readonly Decklist _decklist;
        private IList<GameResult> _gameResults;

        public DeckViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            IGameResultStorage gameResultStorage,
            ILogger logger,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Guid deckID,
            Decklist decklist,
            IEnumerable<GameResult> gameResults = null)
        {
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.ThrowIfNull(nameof(configurationSettings));
            _eventDispatcherFactory = eventDispatcherFactory.ThrowIfNull(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.ThrowIfNull(nameof(eventStreamFactory));
            _gameEventDispatcher = gameEventDispatcher.ThrowIfNull(nameof(gameEventDispatcher));
            _gameResultStorage = gameResultStorage.ThrowIfNull(nameof(gameResultStorage));
            _logger = logger.ThrowIfNull(nameof(logger));
            _viewEventDispatcher = viewEventDispatcher.ThrowIfNull(nameof(viewEventDispatcher));

            _deckID = deckID;
            _decklist = decklist;

            _gameResults = new List<GameResult>(gameResults ?? Enumerable.Empty<GameResult>());
        }

        public string Class => _cardInfoProvider.GetCardInfo(_decklist.HeroID)?.Class;

        public Guid DeckID => _deckID;

        public Decklist Decklist => _decklist;

        public IEnumerable<GameResult> GameResults => _gameResults;

        public int Losses => _gameResults.Count(__gameResult => !__gameResult.GameWon);

        public ICommand TrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _viewEventDispatcher.DispatchEvent(new DeckTrackingEvent());

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

        private sealed class DeckTrackingEvent { }
    }
}
