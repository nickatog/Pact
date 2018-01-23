using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class MainWindowViewModel
    {
        private readonly IDeckStringSerializer _deckStringSerializer;
        private readonly Valkyrie.IEventDispatcher _eventDispatcher;
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IDeckInfoRepository _deckInfoRepository;
        private readonly IGameResultStorage _gameResultStorage;
        private readonly IDecklistSerializer _decklistSerializer;

        public MainWindowViewModel(
            IDeckStringSerializer deckStringSerializer,
            IDecklistSerializer decklistSerializer,
            Valkyrie.IEventDispatcher eventDispatcher,
            ICardInfoProvider cardInfoProvider,
            IDeckInfoRepository deckInfoRepository,
            IGameResultStorage gameResultStorage)
        {
            _decklistSerializer = decklistSerializer.OrThrow(nameof(decklistSerializer));

            _deckStringSerializer =
                deckStringSerializer
                ?? throw new ArgumentNullException(nameof(deckStringSerializer));

            _eventDispatcher =
                eventDispatcher
                ?? throw new ArgumentNullException(nameof(eventDispatcher));

            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _deckInfoRepository =
                deckInfoRepository
                ?? throw new ArgumentNullException(nameof(deckInfoRepository));

            _gameResultStorage =
                gameResultStorage
                ?? throw new ArgumentNullException(nameof(gameResultStorage));

            _configurationSettings.AccountName = "Nickatog";

            _handler =
                (__decklist, __unregister) =>
                {
                    _unregister?.Invoke();

                    var tracker =
                        PlayerDeckTrackerView.GetWindowFor(
                            new PlayerDeckTrackerViewModel(
                                _accountName,
                                __decklist,
                                _eventDispatcher,
                                _cardInfoProvider));

                    tracker.Show();

                    _unregister = __unregister;
                };

            _decks =
                new ObservableCollection<DeckViewModel>(
                    _deckInfoRepository.GetAll()
                    .Select(
                        __deckInfo =>
                            new DeckViewModel(
                                __deckInfo.DeckID,
                                __deckInfo.DeckString,
                                _deckStringSerializer.Deserialize(__deckInfo.DeckString),
                                _handler,
                                _gameResultStorage,
                                _eventDispatcher,
                                _cardInfoProvider.GetCardInfo(_deckStringSerializer.Deserialize(__deckInfo.DeckString).HeroID)?.Class,
                                _configurationSettings,
                                _cardInfoProvider,
                                __deckInfo.GameResults)));
        }

        private Action _unregister;

        private string _accountName = "Nickatog";
        public string AccountName => _accountName;

        private readonly ConfigurationSettings _configurationSettings = new ConfigurationSettings();

        private readonly IList<DeckViewModel> _decks = new ObservableCollection<DeckViewModel>();
        public IEnumerable<DeckViewModel> Decks => _decks;

        private Action<Decklist, Action> _handler;

        public ICommand ImportDeck =>
            new DelegateCommand(
                () =>
                {
                    var view = new DeckImportView(_decklistSerializer) { Owner = MainWindow.Window };
                    if (view.ShowDialog() ?? false)
                    {
                        var deckID = Guid.NewGuid();

                        _decks.Add(
                            new DeckViewModel(
                                deckID,
                                view.DeckString,
                                view.Deck,
                                _handler,
                                _gameResultStorage,
                                _eventDispatcher,
                                _cardInfoProvider.GetCardInfo(view.Deck.HeroID)?.Class,
                                _configurationSettings,
                                _cardInfoProvider));

                        _deckInfoRepository.Save(_decks.Select(__deck => new DeckInfo(__deck.DeckID, __deck.DeckString, __deck.GameResults)));
                    }
                });
    }
}
