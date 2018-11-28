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
        private readonly ISerializer<Models.Client.Decklist> _decklistSerializer;
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

        public DeckManagerViewModel(
            IBackgroundWorkInterface backgroundWorkInterface,
            ICardInfoProvider cardInfoProvider,
            IDeckImportInterface deckImportInterface,
            ISerializer<Models.Client.Decklist> decklistSerializer,
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

            // Start loading pre-existing decks
            Task.Run(
                async () =>
                {
                    IEnumerable<Models.Client.Deck> decks = await _deckRepository.GetAllDecks();

                    _deckViewModels =
                        new ObservableCollection<DeckViewModel>(
                            decks
                            .OrderBy(__deck => __deck.Position)
                            .Select(
                                __deck =>
                                    CreateDeckViewModel(
                                        __deck.DeckID,
                                        DeserializeDecklist(__deck.DeckString),
                                        __deck.Title,
                                        __deck.GameResults)));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeckViewModels)));
                });

            Task.Run(
                async () =>
                {
                    using (IEventStream eventStream = _eventStreamFactory.Create())
                    {
                        eventStream.SeekEnd();

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
                        DeckViewModel deckViewModel =
                            _deckViewModels
                            .FirstOrDefault(__deckViewModel => __deckViewModel.DeckID == __event.DeckID);
                        if (deckViewModel == null)
                            return;
                        
                        _deckViewModels.Remove(deckViewModel);

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

            Models.Client.Decklist DeserializeDecklist(
                string deckstring)
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(deckstring)))
                    return _decklistSerializer.Deserialize(stream).Result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<DeckViewModel> DeckViewModels => _deckViewModels;

        public ICommand ImportDeck =>
            new DelegateCommand(
                async () =>
                {
                    Models.Interface.DeckImportDetail? deckImportDetail = await _deckImportInterface.GetDetail();
                    if (!deckImportDetail.HasValue)
                        return;

                    _deckViewModels.Insert(
                        0,
                        CreateDeckViewModel(
                            Guid.NewGuid(),
                            deckImportDetail.Value.Decklist,
                            deckImportDetail.Value.Title));

                    await SaveDecks();
                });

        private DeckViewModel CreateDeckViewModel(
            Guid deckID,
            Models.Client.Decklist decklist,
            string title,
            IEnumerable<Models.Client.GameResult> gameResults = null)
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

        private Task SaveDecks()
        {
            IEnumerable<Models.Client.DeckDetail> deckDetails =
                _deckViewModels
                .Select(
                    __deckViewModel =>
                    {
                        using (var stream = new MemoryStream())
                        {
                            _decklistSerializer.Serialize(stream, __deckViewModel.Decklist).Wait();

                            stream.Position = 0;

                            using (var reader = new StreamReader(stream))
                            {
                                return
                                    new Models.Client.DeckDetail(
                                        __deckViewModel.DeckID,
                                        __deckViewModel.Title,
                                        reader.ReadToEnd(),
                                        __deckViewModel.Position);
                            }
                        }
                    });

            return _deckRepository.ReplaceAllDecks(deckDetails);
        }
    }
}
