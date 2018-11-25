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
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;
        private readonly IDeckRepository _deckRepository;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultRepository _gameResultRepository;
        private readonly IPlayerDeckTrackerInterface _playerDeckTrackerInterface;
        private readonly IReplaceDeckInterface _replaceDeckInterface;
        private readonly IUserConfirmationInterface _userConfirmationInterface;
        private readonly IEventDispatcher _viewEventDispatcher;

        private Action _deleteCanExecuteChanged;
        private readonly IList<Models.Client.GameResult> _gameResults;
        private readonly Func<DeckViewModel, ushort> _getPosition;
        private bool _isTracking = false;
        private Action _replaceCanExecuteChanged;
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();
        
        public DeckViewModel(
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            ISerializer<Models.Client.Decklist> decklistSerializer,
            IDeckRepository deckRepository,
            IEventDispatcher gameEventDispatcher,
            IGameResultRepository gameResultRepository,
            IPlayerDeckTrackerInterface playerDeckTrackerInterface,
            IReplaceDeckInterface replaceDeckInterface,
            IUserConfirmationInterface userConfirmationInterface,
            IEventDispatcher viewEventDispatcher,
            Guid deckID,
            Models.Client.Decklist decklist,
            Func<DeckViewModel, ushort> getPosition,
            string title,
            IEnumerable<Models.Client.GameResult> gameResults = null)
        {
            _backgroundWorkInterface = backgroundWorkInterface.Require(nameof(backgroundWorkInterface));
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _deckImportInterface = deckImportInterface.Require(nameof(deckImportInterface));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _deckRepository = deckRepository.Require(nameof(deckRepository));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _gameResultRepository = gameResultRepository.Require(nameof(gameResultRepository));
            _playerDeckTrackerInterface = playerDeckTrackerInterface.Require(nameof(playerDeckTrackerInterface));
            _replaceDeckInterface = replaceDeckInterface.Require(nameof(replaceDeckInterface));
            _userConfirmationInterface = userConfirmationInterface.Require(nameof(userConfirmationInterface));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            DeckID = deckID;
            Decklist = decklist;
            _gameResults = (gameResults ?? Enumerable.Empty<Models.Client.GameResult>()).ToList();
            _getPosition = getPosition.Require(nameof(getPosition));
            Title = title ?? string.Empty;
            
            _viewEventHandlers.Add(
                new DelegateEventHandler<ViewEvents.DeckTracking>(
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

        public Models.Client.Decklist Decklist { get; private set; }

        public ICommand Delete =>
            new DelegateCommand(
                async () =>
                {
                    if (!await _userConfirmationInterface.Confirm($"Are you sure you want to delete {Title}?", "Delete", "Cancel"))
                        return;

                    _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));

                    _viewEventDispatcher.DispatchEvent(new ViewCommands.DeleteDeck(DeckID));
                },
                canExecute:
                    () => !IsTracking,
                canExecuteChangedClient:
                    __canExecuteChanged => _deleteCanExecuteChanged = __canExecuteChanged);

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
                    Models.Client.Decklist? decklist = await _replaceDeckInterface.GetDecklist();
                    if (!decklist.HasValue)
                        return;

                    Decklist = decklist.Value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Class)));

                    await SaveDeck();
                },
                canExecute:
                    () => !IsTracking,
                canExecuteChangedClient:
                    __canExecuteChanged => _replaceCanExecuteChanged = __canExecuteChanged);

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
                    _viewEventDispatcher.DispatchEvent(new ViewEvents.DeckTracking(this));

                    _playerDeckTrackerInterface.TrackDeck(Decklist);

                    var recordGameResult =
                        new DelegateEventHandler<Events.GameEnded>(
                            __event =>
                            {
                                var gameResult =
                                    new Models.Client.GameResult(
                                        DateTimeOffset.Now,
                                        __event.GameWon,
                                        _cardInfoProvider.GetCardInfo(__event.OpponentHeroCardID)?.Class);

                                _gameResults.Add(gameResult);

                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Wins)));
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Losses)));

                                _gameResultRepository.AddGameResult(DeckID, gameResult);
                            });

                    _gameEventDispatcher.RegisterHandler(recordGameResult);

                    IEventHandler unregisterHandlers = null;
                    unregisterHandlers =
                        new DelegateEventHandler<ViewEvents.DeckTracking>(
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

                    _viewEventDispatcher.DispatchEvent(new ViewEvents.DeckTracking(null));
                });

        public int Wins => _gameResults.Count(__gameResult => __gameResult.GameWon);

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

        private Task SaveDeck()
        {
            return
                _deckRepository.UpdateDeck(
                    new Models.Client.DeckDetail(
                        DeckID,
                        Title,
                        GetDeckstring(),
                        Position));
        }
    }
}
