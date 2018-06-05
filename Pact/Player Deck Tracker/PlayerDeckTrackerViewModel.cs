#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;
#endregion // Namespaces

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Fields
        private IList<TrackedCardViewModel> _cards;
        private readonly Decklist _decklist;
        private bool? _opponentCoinStatus;
        private int _playerID;

        private readonly IList<Valkyrie.IEventHandler> _gameEventHandlers = new List<Valkyrie.IEventHandler>();
        private readonly IList<Valkyrie.IEventHandler> _viewEventHandlers = new List<Valkyrie.IEventHandler>();
        #endregion // Fields

        #region Constructors
        public PlayerDeckTrackerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Decklist decklist)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _viewEventDispatcher = viewEventDispatcher ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            _decklist = decklist;

            Reset();

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (__event.PlayerID != _playerID)
                            return;

                        if (_cards.Any(__card => string.Equals(__card.CardID, __event.CardID, StringComparison.OrdinalIgnoreCase)))
                            return;

                        int? playerID = _cards.First()?.PlayerID;

                        _cards.Add(new TrackedCardViewModel(_cardInfoProvider, _configurationSettings, _gameEventDispatcher, _viewEventDispatcher, __event.CardID, 1, playerID));

                        _cards =
                            _cards
                            .OrderBy(__card => __card.Cost)
                            .ThenBy(__card => __card.Name)
                            .ToList();

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.GameStarted>(
                    __ => Reset()));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.OpponentCoinLost>(
                    __ => OpponentCoinStatus = false));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __ => OpponentCoinStatus = true));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.PlayerDetermined>(
                    __event => _playerID = __event.PlayerID));

            foreach (Valkyrie.IEventHandler handler in _gameEventHandlers)
                _gameEventDispatcher.RegisterHandler(handler);

            _viewEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.DeckTrackerFontSizeChanged>(
                    __ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FontSize"))));

            foreach (Valkyrie.IEventHandler handler in _viewEventHandlers)
                _viewEventDispatcher.RegisterHandler(handler);
        }
        #endregion // Constructors

        public IEnumerable<TrackedCardViewModel> Cards => _cards;

        public void Cleanup()
        {
            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.UnregisterHandler(__handler));
            _gameEventHandlers.Clear();

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));
            _viewEventHandlers.Clear();

            _cards.ForEach(__card => __card.Cleanup());
        }

        public IConfigurationSettings ConfigurationSettings => _configurationSettings;

        public int Count => _cards.Sum(__card => __card.Count);

        public int FontSize => _configurationSettings.FontSize;

        public bool? OpponentCoinStatus
        {
            get => _opponentCoinStatus;
            private set
            {
                _opponentCoinStatus = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));
            }
        }

        private void Reset()
        {
            OpponentCoinStatus = null;

            _cards.ForEach(__card => __card.Cleanup());

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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
