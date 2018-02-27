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
        private readonly Valkyrie.IEventDispatcher _gameEventDispatcher;

        private readonly string _cardID;
        private int _count;

        private readonly IList<Valkyrie.IEventHandler> _gameEventHandlers = new List<Valkyrie.IEventHandler>();
        private int _playerID;

        public TrackedCardViewModel(
            ICardInfoProvider cardInfoProvider,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            string cardID,
            int count)
        {
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));
            _gameEventDispatcher = gameEventDispatcher.ThrowIfNull(nameof(gameEventDispatcher));

            _cardID = cardID;
            _count = count;

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
        }

        ~TrackedCardViewModel()
        {
            System.Diagnostics.Debug.WriteLine("DESTRUCTOR!");
        }

        public string Class => _cardInfoProvider.GetCardInfo(_cardID)?.Class ?? "<UNKNOWN>";

        public int Cost => _cardInfoProvider.GetCardInfo(_cardID)?.Cost ?? 0;

        public int Count => _count;

        public string Name => _cardInfoProvider.GetCardInfo(_cardID)?.Name ?? "<UNKNOWN>";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
