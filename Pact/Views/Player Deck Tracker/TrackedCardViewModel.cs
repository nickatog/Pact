#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Valkyrie;
using Pact.Extensions.Enumerable;
#endregion // Namespaces

namespace Pact
{
    public sealed class TrackedCardViewModel
        : INotifyPropertyChanged
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _gameEventDispatcher;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Fields
        private int _count;
        private int? _playerID;

        private readonly IList<IEventHandler> _gameEventHandlers = new List<IEventHandler>();
        private readonly IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();
        #endregion // Fields

        #region Constructors
        public TrackedCardViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IEventDispatcher gameEventDispatcher,
            IEventDispatcher viewEventDispatcher,
            string cardID,
            int count,
            int? playerID = null)
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

            CardID = cardID;
            _count = count;
            _playerID = playerID;

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count++;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardEnteredPlayFromDeck>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardOverdrawnFromDeck>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardRemovedFromDeck>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
                            Count--;
                    }));

            _gameEventHandlers.Add(
                new DelegateEventHandler<Events.CardReturnedToDeckFromHand>(
                    __event =>
                    {
                        if (_playerID.Equals(__event.PlayerID) && __event.CardID == CardID)
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
                    __event => _playerID = __event.PlayerID));

            _gameEventHandlers.ForEach(__handler => _gameEventDispatcher.RegisterHandler(__handler));

            _viewEventHandlers.Add(
                new DelegateEventHandler<Events.ConfigurationSettingsSaved>(
                    __event =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardTextOffset)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontSize)));
                    }));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }
        #endregion // Constructors

        public string CardID { get; private set; }

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

        public int? PlayerID => _playerID;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
