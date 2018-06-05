using System;
using System.Collections.Generic;
using System.ComponentModel;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class TrackedCardViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        private readonly string _cardID;
        private int _count;

        private readonly IList<Valkyrie.IEventHandler> _gameEventHandlers = new List<Valkyrie.IEventHandler>();
        private readonly IList<Valkyrie.IEventHandler> _viewEventHandlers = new List<Valkyrie.IEventHandler>();
        private int _playerID;

        public TrackedCardViewModel(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            string cardID,
            int count,
            int? playerID = null)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            _gameEventDispatcher = gameEventDispatcher.Require(nameof(gameEventDispatcher));
            _viewEventDispatcher = viewEventDispatcher ?? throw new ArgumentNullException(nameof(viewEventDispatcher));

            _cardID = cardID;
            _count = count;

            if (playerID.HasValue)
                _playerID = playerID.Value;

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            IncrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            DecrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardEnteredPlayFromDeck>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            DecrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardOverdrawnFromDeck>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            DecrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardRemovedFromDeck>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            DecrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.CardReturnedToDeckFromHand>(
                    __event =>
                    {
                        if (__event.PlayerID == _playerID && __event.CardID == _cardID)
                            IncrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.MulliganOptionPresented>(
                    __event =>
                    {
                        if (__event.CardID == _cardID)
                            DecrementCount();
                    }));

            _gameEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.PlayerDetermined>(
                    __event => _playerID = __event.PlayerID));

            foreach (Valkyrie.IEventHandler handler in _gameEventHandlers)
                _gameEventDispatcher.RegisterHandler(handler);

            _viewEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.DeckTrackerCardTextOffsetChanged>(
                    __event =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardTextOffset"));
                    }));

            _viewEventHandlers.Add(
                new Valkyrie.DelegateEventHandler<Events.DeckTrackerFontSizeChanged>(
                    __event =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FontSize"));
                    }));

            foreach (Valkyrie.IEventHandler handler in _viewEventHandlers)
                _viewEventDispatcher.RegisterHandler(handler);

            void DecrementCount()
            {
                _count--;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }

            void IncrementCount()
            {
                _count++;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }

        public void Cleanup()
        {
            foreach (Valkyrie.IEventHandler handler in _gameEventHandlers)
                _gameEventDispatcher.UnregisterHandler(handler);

            _gameEventHandlers.Clear();

            foreach (Valkyrie.IEventHandler handler in _viewEventHandlers)
                _viewEventDispatcher.UnregisterHandler(handler);

            _viewEventHandlers.Clear();
        }

        ~TrackedCardViewModel()
        {
            System.Diagnostics.Debug.WriteLine("DESTRUCTOR!");
        }

        public string CardID => _cardID;

        public int CardTextOffset => _configurationSettings.CardTextOffset;

        public string Class => _cardInfoProvider.GetCardInfo(_cardID)?.Class ?? "<UNKNOWN>";

        public int Cost => _cardInfoProvider.GetCardInfo(_cardID)?.Cost ?? 0;

        public int Count => _count;

        public int FontSize => _configurationSettings.FontSize;

        public string Name => _cardInfoProvider.GetCardInfo(_cardID)?.Name ?? "<UNKNOWN>";

        public int PlayerID => _playerID;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
