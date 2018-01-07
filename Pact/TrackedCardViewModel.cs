using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class TrackedCardViewModel
        : INotifyPropertyChanged
    {
        private readonly string _cardID;
        private readonly ICardInfoProvider _cardInfoProvider;
        private int _count;
        private int _playerID;

        public TrackedCardViewModel(
            string cardID,
            int count,
            Valkyrie.IEventDispatcher eventDispatcher,
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            // wrap provider with caching mechanism? either here or in DI registration via interceptor?

            _cardID = cardID;
            _count = count;

            // register for events that change the count of this card in the deck
            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.PlayerDetermined>(__event => _playerID = __event.PlayerID));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardDrawnFromDeck>(
                    __event =>
                    {
                        if (__event.PlayerID != _playerID)
                            return;

                        if (__event.CardID != _cardID)
                            return;

                        _count--;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                    }));

            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.CardAddedToDeck>(
                    __event =>
                    {
                        if (__event.PlayerID != _playerID)
                            return;

                        if (__event.CardID != _cardID)
                            return;

                        _count++;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                    }));
        }

        public int Cost => _cardInfoProvider.GetCardInfo(_cardID)?.Cost ?? 0;

        public int Count => _count;

        public string Name => _cardInfoProvider.GetCardInfo(_cardID)?.Name ?? "<NOT FOUND>";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
