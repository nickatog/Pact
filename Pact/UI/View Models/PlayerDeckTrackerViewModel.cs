using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Valkyrie;

using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;
using Pact.Extensions.String;

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IEventDispatcher _viewEventDispatcher;

        private readonly Models.Client.Decklist _decklist;
        private readonly IList<IEventHandler> _gameEventHandlers = new List<IEventHandler>();
        private bool? _opponentCoinStatus;
        private int? _playerID;
        private IList<TrackedCardViewModel> _trackedCardViewModels;
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();

        public PlayerDeckTrackerViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IEventDispatcher gameEventDispatcher,
            IEventDispatcher viewEventDispatcher,
            Models.Client.Decklist decklist)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _decklist = decklist;

            Reset();
            
            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (!_playerID.Equals(__event.PlayerID))
                            return;

                        if (_trackedCardViewModels.Any(__trackedCardViewModel => __trackedCardViewModel.CardID.Eq(__event.CardID)))
                            return;

                        int? playerID = _trackedCardViewModels.First()?.PlayerID;

                        _trackedCardViewModels.Add(CreateCardViewModel(__event.CardID, 1, playerID));

                        _trackedCardViewModels =
                            _trackedCardViewModels
                            .OrderBy(__trackedCardViewModel => __trackedCardViewModel.Cost)
                            .ThenBy(__trackedCardViewModel => __trackedCardViewModel.Name)
                            .ToList();

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackedCardViewModels)));
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

            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.RegisterHandler(__handler));

            _viewEventHandlers.Add(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontSize)))));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Cleanup()
        {
            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.UnregisterHandler(__handler));
            _gameEventHandlers.Clear();

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));
            _viewEventHandlers.Clear();

            _trackedCardViewModels.ForEach(__trackedCardViewModel => __trackedCardViewModel.Cleanup());
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

        public IEnumerable<TrackedCardViewModel> TrackedCardViewModels => _trackedCardViewModels;

        private TrackedCardViewModel CreateCardViewModel(
            string cardID,
            int count,
            int? playerID = null)
        {
            var viewModel =
                new TrackedCardViewModel(
                    _cardInfoProvider,
                    _configurationSource,
                    _gameEventDispatcher,
                    _viewEventDispatcher,
                    cardID,
                    count,
                    playerID);

            viewModel.PropertyChanged +=
                (__, __args) =>
                {
                    if (__args.PropertyName == nameof(TrackedCardViewModel.Count))
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                };

            return viewModel;
        }

        private void Reset()
        {
            OpponentCoinStatus = null;
            _playerID = null;

            _trackedCardViewModels?.ForEach(__trackedCardViewModel => __trackedCardViewModel.Cleanup());

            _trackedCardViewModels =
                _decklist.Cards
                .Select(__decklistCard => CreateCardViewModel(__decklistCard.CardID, __decklistCard.Count))
                .OrderBy(__trackedCardViewModel => __trackedCardViewModel.Cost)
                .ThenBy(__trackedCardViewModel => __trackedCardViewModel.Name)
                .ToList();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackedCardViewModels)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }
}
