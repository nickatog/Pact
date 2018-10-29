using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DeckManagerViewModel
        : INotifyPropertyChanged
    {
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly AsyncSemaphore _deckPersistenceMutex;
        private readonly IDeckRepository _deckRepository;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultRepository _gameResultRepository;
        private readonly ILogger _logger;
        private readonly IPlayerDeckTrackerInterface _playerDeckTrackerInterface;
        private readonly IUserConfirmationInterface _userConfirmationInterface;
        private readonly IEventDispatcher _viewEventDispatcher;

        private IList<DeckViewModel> _deckViewModels;

        public DeckManagerViewModel(
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            IDecklistSerializer decklistSerializer,
            AsyncSemaphore deckPersistenceMutex,
            IDeckRepository deckRepository,
            IEventStream eventStream,
            IEventDispatcher gameEventDispatcher,
            IGameResultRepository gameResultRepository,
            ILogger logger,
            IPlayerDeckTrackerInterface playerDeckTrackerInterface,
            IUserConfirmationInterface userConfirmationInterface,
            IEventDispatcher viewEventDispatcher)
        {
            _backgroundWorkInterface =
                backgroundWorkInterface.Require(nameof(backgroundWorkInterface));

            _cardInfoProvider =
                cardInfoProvider.Require(nameof(cardInfoProvider));

            _deckImportInterface =
                deckImportInterface.Require(nameof(deckImportInterface));

            _decklistSerializer =
                decklistSerializer.Require(nameof(decklistSerializer));

            _deckPersistenceMutex =
                deckPersistenceMutex.Require(nameof(deckPersistenceMutex));

            _deckRepository =
                deckRepository.Require(nameof(deckRepository));

            if (eventStream == null)
                throw new ArgumentNullException(nameof(eventStream));

            _gameEventDispatcher =
                gameEventDispatcher.Require(nameof(gameEventDispatcher));

            _gameResultRepository =
                gameResultRepository.Require(nameof(gameResultRepository));

            _logger =
                logger.Require(nameof(logger));

            _playerDeckTrackerInterface =
                playerDeckTrackerInterface.Require(nameof(playerDeckTrackerInterface));

            _userConfirmationInterface =
                userConfirmationInterface.Require(nameof(userConfirmationInterface));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));

            // Start loading pre-existing decks
            Task.Run(
                async () =>
                {
                    IEnumerable<DeckInfo> deckInfos = await _deckRepository.GetAllDecksAndGameResults();

                    _deckViewModels =
                        new ObservableCollection<DeckViewModel>(
                            deckInfos
                            .OrderBy(__deckInfo => __deckInfo.Position)
                            .Select(
                                __deckInfo =>
                                    CreateDeckViewModel(
                                        __deckInfo.DeckID,
                                        DeserializeDecklist(__deckInfo.DeckString),
                                        __deckInfo.Position,
                                        __deckInfo.Title,
                                        __deckInfo.GameResults)));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Decks)));
                });

            // This event stream should ideally skip all pre-existing events and then begin pumping new events
            // Needs new support added to IEventStream
            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        try { gameEventDispatcher.DispatchEvent(await eventStream.ReadNext()); }
                        catch (Exception ex) { await logger.Write($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"); }
                    }
                });

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<Commands.DeleteDeck>(
                    async __event =>
                    {
                        DeckViewModel deck = _deckViewModels.FirstOrDefault(__deck => __deck.DeckID == __event.DeckID);
                        if (deck == null)
                            return;
                        
                        _deckViewModels.Remove(deck);

                        _viewEventDispatcher.DispatchEvent(new Events.DeckDeleted(deck));

                        await SaveDecks();
                    }));

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<Commands.MoveDeck>(
                    async __event =>
                    {
                        int sourcePosition = __event.SourcePosition;
                        if (sourcePosition > _deckViewModels.Count)
                            return;

                        DeckViewModel sourceDeck = _deckViewModels[sourcePosition];
                        _deckViewModels.RemoveAt(sourcePosition);

                        int targetPosition = __event.TargetPosition;
                        _deckViewModels.Insert(targetPosition, sourceDeck);

                        _viewEventDispatcher.DispatchEvent(new Events.DeckMoved(sourcePosition, targetPosition));

                        await SaveDecks();
                    }));

            Decklist DeserializeDecklist(string text)
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(text)))
                    return _decklistSerializer.Deserialize(stream).Result;
            }
        }

        private DeckViewModel CreateDeckViewModel(
            Guid deckID,
            Decklist decklist,
            int position,
            string title,
            IEnumerable<GameResult> gameResults = null)
        {
            return
                new DeckViewModel(
                    _backgroundWorkInterface,
                    _cardInfoProvider,
                    _deckImportInterface,
                    _decklistSerializer,
                    _deckRepository,
                    _gameEventDispatcher,
                    _gameResultRepository,
                    _playerDeckTrackerInterface,
                    _userConfirmationInterface,
                    _viewEventDispatcher,
                    deckID,
                    decklist,
                    position,
                    title,
                    gameResults);
        }

        public IEnumerable<DeckViewModel> Decks => _deckViewModels;

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

                    DeckViewModel viewModel =
                        CreateDeckViewModel(
                            deckID,
                            deck.Value.Decklist,
                            0,
                            deck.Value.Title);

                    _deckViewModels.Insert(0, viewModel);

                    _viewEventDispatcher.DispatchEvent(new Events.DeckAdded(viewModel));

                    await SaveDecks();
                });

        private async Task SaveDecks()
        {
            IEnumerable<DeckDetails> deckDetails =
                _deckViewModels.Select(
                    __deckViewModel =>
                    {
                        using (var stream = new MemoryStream())
                        {
                            _decklistSerializer.Serialize(stream, __deckViewModel.Decklist).Wait();

                            stream.Position = 0;

                            using (var reader = new StreamReader(stream))
                                return
                                    new DeckDetails(
                                        __deckViewModel.DeckID,
                                        __deckViewModel.Title,
                                        reader.ReadToEnd(),
                                        (UInt16)__deckViewModel.Position);
                        }
                    });

            await _deckRepository.ReplaceDecks(deckDetails).ConfigureAwait(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
