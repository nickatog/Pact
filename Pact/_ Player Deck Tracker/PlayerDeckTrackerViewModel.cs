using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModel
        : INotifyPropertyChanged
    {
        private readonly ICardInfoProvider _cardInfoProvider;

        private IEnumerable<TrackedCardViewModel> _cards;
        private bool? _opponentCoinStatus = null;

        public PlayerDeckTrackerViewModel(
            ICardInfoProvider cardInfoProvider,
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Decklist decklist)
        {
            _cardInfoProvider = cardInfoProvider.ThrowIfNull(nameof(cardInfoProvider));

            Reset();

            gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.GameStarted>(__ => Reset()));

            gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentCoinLost>(
                    __ =>
                    {
                        _opponentCoinStatus = false;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));
                    }));

            gameEventDispatcher.RegisterHandler(
                new Valkyrie.DelegateEventHandler<Events.OpponentReceivedCoin>(
                    __ =>
                    {
                        _opponentCoinStatus = true;

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));
                    }));

            // card added to deck: if not a card that originally started in the deck, add new view model for it

            void Reset()
            {
                _opponentCoinStatus = null;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpponentCoinStatus"));

                _cards =
                    decklist.Cards
                    .Select(__card => new TrackedCardViewModel(__card.CardID, __card.Count, gameEventDispatcher, cardInfoProvider))
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
        }

        public IEnumerable<TrackedCardViewModel> Cards => _cards;

        public int Count => _cards.Sum(__card => __card.Count);

        public bool? OpponentCoinStatus => _opponentCoinStatus;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
