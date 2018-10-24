using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Valkyrie;
using Pact.Extensions.Enumerable;
using Pact.StringExtensions;

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IEventDispatcher _viewEventDispatcher;

        private readonly Decklist _decklist;
        private bool? _opponentCoinStatus;
        private int _playerID;
        private IList<TrackedCardViewModel> _trackedCardViewModels;

        private readonly IList<IEventHandler> _gameEventHandlers = new List<IEventHandler>();
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();

        public PlayerDeckTrackerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage,
            IEventDispatcher gameEventDispatcher,
            IEventDispatcher viewEventDispatcher,
            Decklist decklist)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _gameEventDispatcher =
                gameEventDispatcher
                ?? throw new ArgumentNullException(nameof(gameEventDispatcher));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            _decklist = decklist;

            Reset();

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (__event.PlayerID != _playerID)
                            return;

                        if (_trackedCardViewModels.Any(__trackedCardViewModel => __trackedCardViewModel.CardID.Eq(__event.CardID)))
                            return;

                        int? playerID = _trackedCardViewModels.First()?.PlayerID;

                        TrackedCardViewModel trackedCardViewModel =
                            CreateCardViewModel(
                                __event.CardID,
                                1,
                                playerID);

                        trackedCardViewModel.PropertyChanged +=
                            (__sender, __args) =>
                            {
                                if (__args.PropertyName == nameof(TrackedCardViewModel.Count))
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                            };

                        _trackedCardViewModels.Add(trackedCardViewModel);

                        _trackedCardViewModels =
                            _trackedCardViewModels
                            .OrderBy(__trackedCardViewModel => __trackedCardViewModel.Cost)
                            .ThenBy(__trackedCardViewModel => __trackedCardViewModel.Name)
                            .ToList();

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cards)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.GameStarted>(
                    __ => Reset()));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.OpponentCoinLost>(
                    __ => OpponentCoinStatus = false));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __ => OpponentCoinStatus = true));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.PlayerDetermined>(
                    __event => _playerID = __event.PlayerID));

            foreach (IEventHandler handler in _gameEventHandlers)
                _gameEventDispatcher.RegisterHandler(handler);

            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.ConfigurationSettingsSaved>(
                    __ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontSize)))));

            foreach (IEventHandler handler in _viewEventHandlers)
                _viewEventDispatcher.RegisterHandler(handler);
        }

        public IEnumerable<TrackedCardViewModel> Cards => _trackedCardViewModels;

        public void Cleanup()
        {
            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.UnregisterHandler(__handler));
            _gameEventHandlers.Clear();

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));
            _viewEventHandlers.Clear();

            _trackedCardViewModels.ForEach(__trackedCardViewModel => __trackedCardViewModel.Cleanup());
        }

        private TrackedCardViewModel CreateCardViewModel(
            string cardID,
            int count,
            int? playerID = null)
        {
            return
                new TrackedCardViewModel(
                    _cardInfoProvider,
                    _configurationSource,
                    _gameEventDispatcher,
                    _viewEventDispatcher,
                    cardID,
                    count,
                    playerID);
        }

        public int Count => _trackedCardViewModels.Sum(__trackedCardViewModel => __trackedCardViewModel.Count);

        public int FontSize => _configurationSource.GetSettings().FontSize;

        public bool? OpponentCoinStatus
        {
            get => _opponentCoinStatus;
            private set
            {
                _opponentCoinStatus = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OpponentCoinStatus)));
            }
        }

        private void Reset()
        {
            OpponentCoinStatus = null;

            _trackedCardViewModels.ForEach(__trackedCardViewModel => __trackedCardViewModel.Cleanup());

            _trackedCardViewModels =
                _decklist.Cards
                .Select(__cardInfo => CreateCardViewModel(__cardInfo.CardID, __cardInfo.Count))
                .OrderBy(__trackedCardViewModel => __trackedCardViewModel.Cost)
                .ThenBy(__trackedCardViewModel => __trackedCardViewModel.Name)
                .ToList();

            foreach (TrackedCardViewModel trackedCardViewModel in _trackedCardViewModels)
                trackedCardViewModel.PropertyChanged +=
                    (__, __args) =>
                    {
                        if (__args.PropertyName == nameof(TrackedCardViewModel.Count))
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    };

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cards)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
