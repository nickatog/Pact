using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        private IEnumerable<TrackedCardViewModel> _cards;
        private readonly Decklist _decklist;
        private bool? _opponentCoinStatus;

        private readonly IList<Valkyrie.IEventHandler> _gameEventHandlers = new List<Valkyrie.IEventHandler>();

        public PlayerDeckTrackerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Decklist decklist)
        {
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            _gameEventDispatcher = gameEventDispatcher.ThrowIfNull(nameof(gameEventDispatcher));
            _viewEventDispatcher = viewEventDispatcher ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            _decklist = decklist;

            Reset();

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.GameStarted>(__ => Reset()));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.OpponentCoinLost>(
                    __ =>
                    {
                        _opponentCoinStatus = false;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __ =>
                    {
                        _opponentCoinStatus = true;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));
                    }));

            foreach (Valkyrie.IEventHandler handler in _gameEventHandlers)
                _gameEventDispatcher.RegisterHandler(handler);

            // card added to deck: if not a card that originally started in the deck, add new view model for it
        }

        public void Cleanup()
        {
            _gameEventHandlers.ForEach(__ => _gameEventDispatcher.UnregisterHandler(__));

            _gameEventHandlers.Clear();

            _cards.ForEach(__ => __.Cleanup());
        }

        private void Reset()
        {
            _opponentCoinStatus = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));

            _cards.ForEach(__ => __.Cleanup());

            _cards =
                _decklist.Cards
                .Select(__card => new TrackedCardViewModel(_cardInfoProvider, _configurationSettings, _gameEventDispatcher, _viewEventDispatcher, __card.CardID, __card.Count))
                .OrderBy(__trackedCard => __trackedCard.Cost)
                .ThenBy(__trackedCard => __trackedCard.Name)
                .ToList();

            foreach (TrackedCardViewModel card in _cards)
                card.PropertyChanged +=
                    (__sender, __args) =>
                    {
                        if (__args.PropertyName == "Count")
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                    };

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
        }

        ~PlayerDeckTrackerViewModel()
        {
            System.Diagnostics.Debug.WriteLine("PlayerDeckTrackerViewModel::Destructor()");
        }

        public IEnumerable<TrackedCardViewModel> Cards => _cards;

        public IConfigurationSettings ConfigurationSettings => _configurationSettings;

        public int Count => _cards.Sum(__card => __card.Count);

        public bool? OpponentCoinStatus => _opponentCoinStatus;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
