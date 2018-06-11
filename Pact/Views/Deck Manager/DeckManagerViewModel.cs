#region Namespaces
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class DeckManagerViewModel
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IDeckImportInterface _deckImportInterface;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IDecklistSerializer _decklistSerializer;
        private readonly AsyncSemaphore _deckPersistenceMutex;
        private readonly IDeckViewModelFactory _deckViewModelFactory;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Fields
        private readonly IList<DeckViewModel> _deckViewModels;
        #endregion // Fields

        #region Constructors
        public DeckManagerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IDeckImportInterface deckImportInterface,
            IDeckInfoRepository deckInfoRepository,
            IDecklistSerializer decklistSerializer,
            AsyncSemaphore deckPersistenceMutex,
            IDeckViewModelFactory deckViewModelFactory,
            IEventStream eventStream,
            IEventDispatcher gameEventDispatcher,
            ILogger logger,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _configurationSettings =
                configurationSettings
                ?? throw new ArgumentNullException(nameof(configurationSettings));

            _deckImportInterface =
                deckImportInterface
                ?? throw new ArgumentNullException(nameof(deckImportInterface));

            _deckInfoRepository =
                deckInfoRepository
                ?? throw new ArgumentNullException(nameof(deckInfoRepository));

            _decklistSerializer =
                decklistSerializer
                ?? throw new ArgumentNullException(nameof(decklistSerializer));

            _deckPersistenceMutex =
                deckPersistenceMutex
                ?? throw new ArgumentNullException(nameof(deckPersistenceMutex));

            _deckViewModelFactory =
                deckViewModelFactory
                ?? throw new ArgumentNullException(nameof(deckViewModelFactory));

            if (eventStream == null)
                throw new ArgumentNullException(nameof(eventStream));

            if (gameEventDispatcher == null)
                throw new ArgumentNullException(nameof(gameEventDispatcher));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

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

            _deckViewModels =
                new ObservableCollection<DeckViewModel>(
                    _deckInfoRepository.GetAll().Result
                    .OrderBy(__deckInfo => __deckInfo.Position)
                    .Select(
                        __deckInfo =>
                            _deckViewModelFactory.Create(
                                __deck => _deckViewModels.IndexOf(__deck),
                                __deckInfo.DeckID,
                                DeserializeDecklist(__deckInfo.DeckString),
                                __deckInfo.Title,
                                __deckInfo.GameResults)));

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<Commands.DeleteDeck>(
                    async __event =>
                    {
                        DeckViewModel deck = _deckViewModels.FirstOrDefault(__deck => __deck.DeckID == __event.DeckID);
                        if (deck == null)
                            return;
                        
                        _deckViewModels.Remove(deck);

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

                        _deckViewModels.Insert(__event.TargetPosition, sourceDeck);

                        await SaveDecks();
                    }));

            Decklist DeserializeDecklist(string text)
            {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(text)))
                    return _decklistSerializer.Deserialize(stream).Result;
            }
        }
        #endregion // Constructors

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
                        _deckViewModelFactory.Create(
                            __deck => _deckViewModels.IndexOf(__deck),
                            deckID,
                            deck.Value.Decklist,
                            deck.Value.Title);

                    _deckViewModels.Insert(0, viewModel);

                    await SaveDecks();
                });

        private async Task SaveDecks()
        {
            using (await _deckPersistenceMutex.WaitAsync().ConfigureAwait(false))
            {
                IEnumerable<DeckInfo> deckInfos =
                    _deckViewModels.Select(
                        __deck =>
                        {
                            using (var stream = new MemoryStream())
                            {
                                _decklistSerializer.Serialize(stream, __deck.Decklist);

                                stream.Position = 0;

                                using (var reader = new StreamReader(stream))
                                    return new DeckInfo(__deck.DeckID, reader.ReadToEnd(), __deck.Title, (UInt16)_deckViewModels.IndexOf(__deck), __deck.GameResults);
                            }
                        });

                await _deckInfoRepository.ReplaceAll(deckInfos).ConfigureAwait(false);
            }
        }
    }
}
