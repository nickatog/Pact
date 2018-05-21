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
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly AsyncSemaphore _deckPersistenceMutex;
        private readonly IDeckTrackerInterface _deckTrackerInterface;
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly ILogger _logger;
        private readonly IWaitInterface _notifyWaiter;
        private readonly IUserConfirmationInterface _userConfirmation;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        // try to eliminate the need for the first couple delegates by sending out events to get handled elsewhere (deck manager view model)?
        private readonly Action<DeckViewModel, int> _emplaceDeck;
        private readonly Func<DeckViewModel, int> _findDeckPosition;

        private readonly Guid _deckID;
        private Decklist _decklist;
        private IList<GameResult> _gameResults;
        private string _title;

        public DeckViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDeckImportInterface deckImportInterface,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            AsyncSemaphore deckPersistenceMutex,
            IDeckTrackerInterface deckTrackerInterface,
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            ILogger logger,
            IWaitInterface notifyWaiter,
            IUserConfirmationInterface userConfirmation,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Action<DeckViewModel, int> emplaceDeck,
            Func<DeckViewModel, int> findPosition,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults = null)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _deckImportInterface = deckImportInterface.Require(nameof(deckImportInterface));
            _deckInfoRepository = deckInfoRepository.Require(nameof(deckInfoRepository));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _deckPersistenceMutex = deckPersistenceMutex.Require(nameof(deckPersistenceMutex));
            _deckTrackerInterface = deckTrackerInterface.Require(nameof(deckTrackerInterface));
            _emplaceDeck = emplaceDeck.Require(nameof(emplaceDeck));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _findDeckPosition = findPosition.Require(nameof(findPosition));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _logger = logger.Require(nameof(logger));
            _notifyWaiter = notifyWaiter.Require(nameof(notifyWaiter));
            _userConfirmation = userConfirmation.Require(nameof(userConfirmation));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
            
            _deckID = deckID;
            _decklist = decklist;
            _title = title ?? string.Empty;

            Title = _title;

            _gameResults = new List<GameResult>(gameResults ?? Enumerable.Empty<GameResult>());

            // Needs to get unregistered when deck is deleted so the view model can get garbage collected
            _viewEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<DeckTrackingEvent>(
                    __event =>
                    {
                        IsTracking = __event.DeckViewModel == this;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTracking"));

                        _canDelete = !IsTracking;

                        _deleteCanExecuteChanged?.Invoke();

                        _canReplace = !IsTracking;
                        _replaceCanExecuteChanged?.Invoke();
                    }));
        }

        private bool _canDelete = true;

        public string Class => _cardInfoProvider.GetCardInfo(_decklist.HeroID)?.Class;

        public ICommand CopyDeck =>
            new DelegateCommand(
                async () =>
                {
                    if (!await _userConfirmation.Confirm("Copy deck to clipboard?", "Yes", "No"))
                        return;

                    byte[] bytes = null;

                    await _notifyWaiter.Perform(
                        () =>
                        {
                            using (var stream = new MemoryStream())
                            {
                                _decklistSerializer.Serialize(stream, _decklist).Wait();

                                stream.Position = 0;

                                bytes = new byte[stream.Length];
                                stream.Read(bytes, 0, (int)stream.Length);

                                Thread.Sleep(1000);
                            }
                        });

                    Clipboard.SetText(Encoding.Default.GetString(bytes));
                });


        public Guid DeckID => _deckID;

        public Decklist Decklist => _decklist;

        private Action _deleteCanExecuteChanged;
        public ICommand Delete =>
            new DelegateCommand(
                async () =>
                {
                    //if (!await _userConfirmation.Confirm($"Are you sure you want to delete {Title}?", "Delete", "Cancel"))
                    //    return;

                    _viewEventDispatcher.DispatchEvent(new Events.DeckDeleted(_deckID));
                },
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

        public bool IsTracking { get; private set; }

        public int Losses => _gameResults.Count(__gameResult => !__gameResult.GameWon);

        public int Position => _findDeckPosition(this);

        private bool _canReplace = true;
        private Action _replaceCanExecuteChanged;
        public ICommand Replace =>
            new DelegateCommand(
                async () =>
                {
                    DeckImportDetails? deckImportResult = await _deckImportInterface.GetDecklist();
                    if (!deckImportResult.HasValue)
                        return;

                    _decklist = deckImportResult.Value.Decklist;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Class"));

                    await SaveDeckToRepository();
                },
                canExecute:
                    () => _canReplace,
                canExecuteChangedClient:
                    __canExecuteChanged => _replaceCanExecuteChanged = __canExecuteChanged);

        public ICommand SaveDeckTitle =>
            new DelegateCommand(
                async () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));

                    await SaveDeckToRepository();
                });

        public string Title { get; set; }

        public ICommand TrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _viewEventDispatcher.DispatchEvent(new DeckTrackingEvent(this));

                    Valkyrie.IEventDispatcher trackerEventDispatcher = _eventDispatcherFactory.Create();

                    _deckTrackerInterface.StartTracking(trackerEventDispatcher, _viewEventDispatcher, _decklist);

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
                            async __event =>
                            {
                                var timestamp = DateTime.UtcNow;

                                string opponentClass = _cardInfoProvider.GetCardInfo(__event.OpponentHeroCardID)?.Class;

                                var gameResult = new GameResult(timestamp, __event.GameWon, opponentClass);

                                _gameResults.Add(gameResult);

                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Wins"));
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Losses"));

                                await SaveDeckToRepository();
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

                    IsTracking = true;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTracking"));
                });

        public ICommand UntrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _deckTrackerInterface.Close();

                    IsTracking = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTracking"));

                    _canDelete = true;
                    _deleteCanExecuteChanged?.Invoke();

                    _canReplace = true;
                    _replaceCanExecuteChanged?.Invoke();
                });

        public Valkyrie.IEventDispatcher ViewEventDispatcher => _viewEventDispatcher;

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task SaveDeckToRepository()
        {
            using (await _deckPersistenceMutex.WaitAsync().ConfigureAwait(false))
            {
                DeckInfo deckInfo;

                using (var stream = new MemoryStream())
                {
                    await _decklistSerializer.Serialize(stream, _decklist);

                    stream.Position = 0;

                    using (var reader = new StreamReader(stream))
                        deckInfo = new DeckInfo(_deckID, reader.ReadToEnd(), Title, (ushort)_findDeckPosition(this), _gameResults);
                }

                await _deckInfoRepository.Save(deckInfo).ConfigureAwait(false);
            }
        }

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
