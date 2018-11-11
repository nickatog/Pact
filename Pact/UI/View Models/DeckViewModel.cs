using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        #region Private members
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IDeckRepository _deckRepository;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultRepository _gameResultRepository;
        private readonly IPlayerDeckTrackerInterface _playerDeckTrackerInterface;
        private readonly IReplaceDeckInterface _replaceDeckInterface;
        private readonly IUserConfirmationInterface _userConfirmationInterface;
        private readonly IEventDispatcher _viewEventDispatcher;

        private Action _deleteCanExecuteChanged;
        private readonly IList<GameResult> _gameResults;
        private readonly Func<DeckViewModel, ushort> _getPosition;
        private bool _isTracking = false;
        private Action _replaceCanExecuteChanged;
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();
        #endregion // Members
        
        public DeckViewModel(
            #region Dependency assignment
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            IDecklistSerializer decklistSerializer,
            IDeckRepository deckRepository,
            IEventDispatcher gameEventDispatcher,
            IGameResultRepository gameResultRepository,
            IPlayerDeckTrackerInterface playerDeckTrackerInterface,
            IReplaceDeckInterface replaceDeckInterface,
            IUserConfirmationInterface userConfirmationInterface,
            IEventDispatcher viewEventDispatcher,
            Guid deckID,
            Decklist decklist,
            Func<DeckViewModel, ushort> getPosition,
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

            _replaceDeckInterface =
                replaceDeckInterface.Require(nameof(replaceDeckInterface));

            _userConfirmationInterface =
                userConfirmationInterface.Require(nameof(userConfirmationInterface));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));

            DeckID = deckID;
            Decklist = decklist;
            _gameResults = (gameResults ?? Enumerable.Empty<GameResult>()).ToList();

            _getPosition =
                getPosition ?? throw new ArgumentNullException(nameof(getPosition));

            Title = title ?? string.Empty;

            #endregion // Dependencies
            
            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.DeckTracking>(
                    __event =>
                    {
                        IsTracking = __event.DeckViewModel == this;

                        _deleteCanExecuteChanged?.Invoke();
                        _replaceCanExecuteChanged?.Invoke();
                    }));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Class => _cardInfoProvider.GetCardInfo(Decklist.HeroID)?.Class;

        public ICommand CopyDeck =>
            new DelegateCommand(
                () =>
                {
                    _backgroundWorkInterface.Perform(
                        async __setStatus =>
                        {
                            Clipboard.SetText(GetDeckstring());

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
                    () => !IsTracking,
                canExecuteChangedClient:
                    __canExecuteChanged => _deleteCanExecuteChanged = __canExecuteChanged);

        private string GetDeckstring()
        {
            using (var stream = new MemoryStream())
            {
                _decklistSerializer.Serialize(stream, Decklist).Wait();

                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

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

        public ushort Position => _getPosition(this);

        public ICommand Replace =>
            new DelegateCommand(
                async () =>
                {
                    Decklist? replacementDecklist = await _replaceDeckInterface.GetReplacementDecklist();
                    if (!replacementDecklist.HasValue)
                        return;

                    Decklist = replacementDecklist.Value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));

                    await SaveDeck();
                },
                canExecute:
                    () => !IsTracking,
                canExecuteChangedClient:
                    __canExecuteChanged => _replaceCanExecuteChanged = __canExecuteChanged);

        private Task SaveDeck()
        {
            return
                _deckRepository.UpdateDeck(
                    new DeckDetails(
                        DeckID,
                        Title,
                        GetDeckstring(),
                        Position));
        }

        public ICommand SaveDeckTitle =>
            new DelegateCommand(
                async () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));

                    await SaveDeck();
                });

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

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);
    }
}
