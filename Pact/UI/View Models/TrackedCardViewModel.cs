using System.Collections.Generic;
using System.ComponentModel;

using Valkyrie;

using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class TrackedCardViewModel
        : INotifyPropertyChanged
    {
        #region Private members
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IEventDispatcher _viewEventDispatcher;

        private int _count;
        private readonly IList<IEventHandler> _gameEventHandlers = new List<IEventHandler>();
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();
        #endregion // Private members

        public TrackedCardViewModel(
            #region Dependency assignments
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IEventDispatcher gameEventDispatcher,
            IEventDispatcher viewEventDispatcher,
            string cardID,
            int count,
            int? playerID = null)
        {
            _cardInfoProvider =
                cardInfoProvider.Require(nameof(cardInfoProvider));

            _configurationSource =
                configurationSource.Require(nameof(configurationSource));

            _gameEventDispatcher =
                gameEventDispatcher.Require(nameof(gameEventDispatcher));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));

            CardID = cardID;
            _count = count;
            PlayerID = playerID;
            #endregion // Dependency assignments

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count++;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardEnteredPlayFromDeck>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardOverdrawnFromDeck>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardRemovedFromDeck>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardReturnedToDeckFromHand>(
                    __event =>
                    {
                        if (PlayerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count++;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.MulliganOptionPresented>(
                    __event =>
                    {
                        if (__event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.PlayerDetermined>(
                    __event => PlayerID = __event.PlayerID));

            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.RegisterHandler(__handler));

            _viewEventHandlers.Add(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __event =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardTextOffset)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontSize)));
                    }));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string CardID { get; }

        public int CardTextOffset => _configurationSource.GetSettings().CardTextOffset;

        public string Class => _cardInfoProvider.GetCardInfo(CardID)?.Class ?? "<UNKNOWN>";

        public void Cleanup()
        {
            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.UnregisterHandler(__handler));
            _gameEventHandlers.Clear();

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));
            _viewEventHandlers.Clear();
        }

        public int Cost => _cardInfoProvider.GetCardInfo(CardID)?.Cost ?? 0;

        public int Count
        {
            get => _count;
            private set
            {
                _count = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public int FontSize => _configurationSource.GetSettings().FontSize;

        public string Name => _cardInfoProvider.GetCardInfo(CardID)?.Name ?? "<UNKNOWN>";

        public int? PlayerID { get; private set; }
    }
}
