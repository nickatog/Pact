using System.Collections.Generic;
using System.Linq;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class LocalDatabaseCardInfoProvider
        : ICardInfoProvider
    {
        private readonly ICardDatabase _cardDatabase;
        private IDictionary<int, Models.Data.Card> _cardByDatabaseID;
        private IDictionary<string, Models.Data.Card> _cardByID;
        private readonly IEventDispatcher _eventDispatcher;

        public LocalDatabaseCardInfoProvider(
            ICardDatabase cardDatabase,
            IEventDispatcher eventDispatcher)
        {
            _cardDatabase = cardDatabase.Require(nameof(cardDatabase));
            _eventDispatcher = eventDispatcher.Require(nameof(eventDispatcher));

            LoadCards();

            _eventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewEvents.CardDatabaseUpdated>(
                    __ => LoadCards()));

            void LoadCards()
            {
                IEnumerable<Models.Data.Card> cardInfos = _cardDatabase.GetCards().Result;

                _cardByDatabaseID = cardInfos.ToDictionary(__cardInfo => __cardInfo.dbfId);
                _cardByID = cardInfos.ToDictionary(__cardInfo => __cardInfo.id);
            }
        }

        Models.Client.CardInfo? ICardInfoProvider.GetCardInfo(
            string cardID)
        {
            if (_cardByID.TryGetValue(cardID, out Models.Data.Card card))
                return CreateCardInfo(card);

            return null;
        }

        Models.Client.CardInfo? ICardInfoProvider.GetCardInfo(
            int databaseID)
        {
            if (_cardByDatabaseID.TryGetValue(databaseID, out Models.Data.Card card))
                return CreateCardInfo(card);

            return null;
        }

        private static Models.Client.CardInfo CreateCardInfo(
            Models.Data.Card card)
        {
            return
                new Models.Client.CardInfo(
                    card.name,
                    card.cardClass,
                    card.cost,
                    card.id,
                    card.dbfId);
        }
    }
}
