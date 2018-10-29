using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Valkyrie;

using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class DeckViewModel
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IDeckRepository _deckRepository;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultRepository _gameResultRepository;
        private readonly IPlayerDeckTrackerInterface _playerDeckTrackerInterface;
        private readonly IUserConfirmationInterface _userConfirmationInterface;
        private readonly IEventDispatcher _viewEventDispatcher;

        private bool _canDelete = true;
        private bool _canReplace = true;
        private Action _deleteCanExecuteChanged;
        private readonly IList<GameResult> _gameResults;
        private bool _isTracking = false;
        private Action _replaceCanExecuteChanged;
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();

        public DeckViewModel(
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            IDecklistSerializer decklistSerializer,
            IDeckRepository deckRepository,
            IEventDispatcher gameEventDispatcher,
            IGameResultRepository gameResultRepository,
            IPlayerDeckTrackerInterface playerDeckTrackerInterface,
            IUserConfirmationInterface userConfirmationInterface,
            IEventDispatcher viewEventDispatcher,
            Guid deckID,
            Decklist decklist,
            int position,
            string title,
            IEnumerable<GameResult> gameResults = null)
        {
            _backgroundWorkInterface =
                backgroundWorkInterface.Require(nameof(backgroundWorkInterface));

            _cardInfoProvider =
                cardInfoProvider.Require(nameof(cardInfoProvider));

            _deckImportInterface =
                deckImportInterface.Require(nameof(deckImportInterface));

            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));

            _deckRepository =
                deckRepository.Require(nameof(deckRepository));

            _gameEventDispatcher =
                gameEventDispatcher.Require(nameof(gameEventDispatcher));

            _gameResultRepository =
                gameResultRepository.Require(nameof(gameResultRepository));

            _playerDeckTrackerInterface =
                playerDeckTrackerInterface.Require(nameof(playerDeckTrackerInterface));

            _userConfirmationInterface =
                userConfirmationInterface.Require(nameof(userConfirmationInterface));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));

            DeckID = deckID;
            Decklist = decklist;
            _gameResults = (gameResults ?? Enumerable.Empty<GameResult>()).ToList();
            Position = position;
            Title = title ?? string.Empty;

            // Listen for decks to be added and adjust the current deck's position
            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.DeckAdded>(
                    __event =>
                    {
                        if (__event.DeckViewModel == this)
                            return;

                        if (__event.DeckViewModel.Position <= Position)
                            Position++;
                    }));

            // Listen for decks to be deleted and adjust the current deck's position
            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.DeckDeleted>(
                    __event =>
                    {
                        if (__event.DeckViewModel.Position < Position)
                            Position--;
                    }));

            // Listen for decks to move positions and adjust the current deck's position
            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.DeckMoved>(
                    __event =>
                    {
                        int sourcePosition = __event.SourcePosition;
                        int targetPosition = __event.TargetPosition;

                        if (sourcePosition == Position)
                            Position = targetPosition;
                        else if (targetPosition >= Position && sourcePosition < Position)
                            Position--;
                        else if (targetPosition <= Position && sourcePosition > Position)
                            Position++;
                    }));

            // Listen for decks to start tracking and set the current deck's tracking status
            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.DeckTracking>(
                    __event =>
                    {
                        IsTracking = __event.DeckViewModel == this;

                        _canDelete = !IsTracking;
                        _deleteCanExecuteChanged?.Invoke();

                        _canReplace = !IsTracking;
                        _replaceCanExecuteChanged?.Invoke();
                    }));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }

        public string Class => _cardInfoProvider.GetCardInfo(Decklist.HeroID)?.Class;

        public ICommand CopyDeck =>
            new DelegateCommand(
                () =>
                {
                    _backgroundWorkInterface.Perform(
                        async __setStatus =>
                        {
                            byte[] bytes = null;

                            using (var stream = new MemoryStream())
                            {
                                await _decklistSerializer.Serialize(stream, Decklist);

                                stream.Position = 0;

                                bytes = new byte[stream.Length];
                                stream.Read(bytes, 0, (int)stream.Length);
                            }

                            Clipboard.SetText(Encoding.Default.GetString(bytes));

                            __setStatus?.Invoke("Deck copied to clipboard!");

                            await Task.Delay(500);
                        },
                        750);
                });

        public Guid DeckID { get; }

        public Decklist Decklist { get; private set; }

        public ICommand Delete =>
            new DelegateCommand(
                async () =>
                {
                    if (!await _userConfirmationInterface.Confirm($"Are you sure you want to delete {Title}?", "Delete", "Cancel"))
                        return;

                    _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));

                    _viewEventDispatcher.DispatchEvent(new Commands.DeleteDeck(DeckID));
                },
                canExecute:
                    () => _canDelete,
                canExecuteChangedClient:
                    __canExecuteChanged => _deleteCanExecuteChanged = __canExecuteChanged);

        // This won't need to be public if the deck storage abstractions are separated into deck property and game results interfaces
        public IEnumerable<GameResult> GameResults => _gameResults;

        public bool IsTracking
        {
            get => _isTracking;
            private set
            {
                _isTracking = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTracking)));
            }
        }

        public int Losses => _gameResults.Count(__gameResult => !__gameResult.GameWon);

        public int Position { get; private set; }

        public ICommand Replace =>
            new DelegateCommand(
                async () =>
                {
                    DeckImportDetails? deckImportResult = await _deckImportInterface.GetDecklist();
                    if (!deckImportResult.HasValue)
                        return;

                    Decklist = deckImportResult.Value.Decklist;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));

                    await SaveDeck();
                },
                canExecute:
                    () => _canReplace,
                canExecuteChangedClient:
                    __canExecuteChanged => _replaceCanExecuteChanged = __canExecuteChanged);

        public ICommand SaveDeckTitle =>
            new DelegateCommand(
                async () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));

                    await SaveDeck();
                });

        private async Task SaveDeck()
        {
            DeckDetails deckDetails;

            using (var stream = new MemoryStream())
            {
                await _decklistSerializer.Serialize(stream, Decklist).ConfigureAwait(false);

                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                    deckDetails = new DeckDetails(DeckID, Title, reader.ReadToEnd(), (ushort)Position);
            }

            await _deckRepository.UpdateDeck(deckDetails).ConfigureAwait(false);
        }

        public string Title { get; set; }

        public ICommand TrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _viewEventDispatcher.DispatchEvent(new Events.DeckTracking(this));

                    _playerDeckTrackerInterface.TrackDeck(Decklist);

                    var recordGameResult =
                        new DelegateEventHandler<Events.GameEnded>(
                            __event =>
                            {
                                var timestamp = DateTime.UtcNow;

                                string opponentClass = _cardInfoProvider.GetCardInfo(__event.OpponentHeroCardID)?.Class;

                                var gameResult = new GameResult(timestamp, __event.GameWon, opponentClass);

                                _gameResults.Add(gameResult);

                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wins)));
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Losses)));

                                _gameResultRepository.AddGameResult(DeckID, gameResult);
                            });

                    _gameEventDispatcher.RegisterHandler(recordGameResult);

                    IEventHandler unregisterHandlers = null;
                    unregisterHandlers =
                        new DelegateEventHandler<Events.DeckTracking>(
                            __event =>
                            {
                                _gameEventDispatcher.UnregisterHandler(recordGameResult);
                                _viewEventDispatcher.UnregisterHandler(unregisterHandlers);
                            });

                    _viewEventDispatcher.RegisterHandler(unregisterHandlers);

                    IsTracking = true;
                });

        public ICommand UntrackDeck =>
            new DelegateCommand(
                () =>
                {
                    _playerDeckTrackerInterface.Close();

                    _viewEventDispatcher.DispatchEvent(new Events.DeckTracking(null));
                });

        public IEventDispatcher ViewEventDispatcher => _viewEventDispatcher;

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);
    }
}
