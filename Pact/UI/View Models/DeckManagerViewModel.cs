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
        #region Private members
        private readonly IBackgroundWorkInterface _backgroundWorkInterface;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly IDeckRepository _deckRepository;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IGameResultRepository _gameResultRepository;
        private readonly ILogger _logger;
        private readonly IPlayerDeckTrackerInterface _playerDeckTrackerInterface;
        private readonly IReplaceDeckInterface _replaceDeckInterface;
        private readonly IUserConfirmationInterface _userConfirmationInterface;
        private readonly IEventDispatcher _viewEventDispatcher;

        private IList<DeckViewModel> _deckViewModels;
        #endregion // Private members

        public DeckManagerViewModel(
            #region Dependency assignment
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            IDecklistSerializer decklistSerializer,
            IDeckRepository deckRepository,
            IEventStreamFactory eventStreamFactory,
            IEventDispatcher gameEventDispatcher,
            IGameResultRepository gameResultRepository,
            ILogger logger,
            IPlayerDeckTrackerInterface playerDeckTrackerInterface,
            IReplaceDeckInterface replaceDeckInterface,
            IUserConfirmationInterface userConfirmationInterface,
            IEventDispatcher viewEventDispatcher)
        {
            _backgroundWorkInterface = backgroundWorkInterface.Require(nameof(backgroundWorkInterface));
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _deckImportInterface = deckImportInterface.Require(nameof(deckImportInterface));
            _decklistSerializer = decklistSerializer.Require(nameof(decklistSerializer));
            _deckRepository = deckRepository.Require(nameof(deckRepository));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _gameResultRepository = gameResultRepository.Require(nameof(gameResultRepository));
            _logger = logger.Require(nameof(logger));
            _playerDeckTrackerInterface = playerDeckTrackerInterface.Require(nameof(playerDeckTrackerInterface));
            _replaceDeckInterface = replaceDeckInterface.Require(nameof(replaceDeckInterface));
            _userConfirmationInterface = userConfirmationInterface.Require(nameof(userConfirmationInterface));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
            #endregion // Dependency assignment

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

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeckViewModels)));
                });

            // This event stream should ideally skip all pre-existing events and then begin pumping new events
            // Needs new support added to IEventStream
            Task.Run(
                async () =>
                {
                    using (IEventStream eventStream = _eventStreamFactory.Create())
                    {
                        while (true)
                        {
                            try { _gameEventDispatcher.DispatchEvent(await eventStream.ReadNext()); }
                            catch (Exception ex) { await _logger.Write($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"); }
                        }
                    }
                });

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewCommands.DeleteDeck>(
                    async __event =>
                    {
                        DeckViewModel deck = _deckViewModels.FirstOrDefault(__deck => __deck.DeckID == __event.DeckID);
                        if (deck == null)
                            return;
                        
                        _deckViewModels.Remove(deck);

                        await SaveDecks();
                    }));

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewCommands.MoveDeck>(
                    async __event =>
                    {
                        ushort sourcePosition = __event.SourcePosition;
                        if (sourcePosition > _deckViewModels.Count)
                            return;

                        DeckViewModel sourceDeck = _deckViewModels[sourcePosition];
                        _deckViewModels.RemoveAt(sourcePosition);

                        ushort targetPosition = __event.TargetPosition;
                        _deckViewModels.Insert(targetPosition, sourceDeck);

                        await SaveDecks();
                    }));

            Decklist DeserializeDecklist(
                string deckstring)
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(deckstring)))
                    return _decklistSerializer.Deserialize(stream).Result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                    _replaceDeckInterface,
                    _userConfirmationInterface,
                    _viewEventDispatcher,
                    deckID,
                    decklist,
                    __deckViewModel => (ushort)_deckViewModels.IndexOf(__deckViewModel),
                    title,
                    gameResults);
        }

        public IEnumerable<DeckViewModel> DeckViewModels => _deckViewModels;

        public ICommand ImportDeck =>
            new DelegateCommand(
                async () =>
                {
                    DeckImportDetails? deck = await _deckImportInterface.GetDeckImportDetails();
                    if (!deck.HasValue)
                        return;

                    _deckViewModels.Insert(
                        0,
                        CreateDeckViewModel(
                            Guid.NewGuid(),
                            deck.Value.Decklist,
                            0,
                            deck.Value.Title));

                    await SaveDecks();
                });

        private Task SaveDecks()
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
                                        __deckViewModel.Position);
                        }
                    });

            return _deckRepository.ReplaceDecks(deckDetails);
        }
    }
}
