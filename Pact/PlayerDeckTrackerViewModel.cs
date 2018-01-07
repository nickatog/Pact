using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private IEnumerable<TrackedCardViewModel> _cards;

        public PlayerDeckTrackerViewModel(
            Decklist decklist,
            Valkyrie.IEventDispatcher eventDispatcher,
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            ResetTrackedCards();

            // register for events that would alter the deck contents (or whatever else we're tracking)

            // game started: reset tracked cards to original decklist
            eventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.GameStarted>(__ => ResetTrackedCards()));

            // card added to deck: if not a card that originally started in the deck, add new view model for it

            void ResetTrackedCards()
            {
                _cards =
                    decklist.Cards
                    .Select(__card => new TrackedCardViewModel(__card.CardID, __card.Count, eventDispatcher, cardInfoProvider))
                    .OrderBy(__trackedCard => __trackedCard.Cost)
                    .ThenBy(__trackedCard => __trackedCard.Name)
                    .ToList();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Cards"));
            }
        }

        public IEnumerable<TrackedCardViewModel> Cards => _cards;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
