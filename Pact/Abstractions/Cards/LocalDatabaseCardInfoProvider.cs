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
        private IDictionary<int, CardInfo> _cardInfoByDatabaseID;
        private IDictionary<string, CardInfo> _cardInfoByID;
        private readonly IEventDispatcher _viewEventDispatcher;

        public LocalDatabaseCardInfoProvider(
            ICardDatabase cardDatabase,
            IEventDispatcher viewEventDispatcher)
        {
            _cardDatabase = cardDatabase.Require(nameof(cardDatabase));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            LoadCards();

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<ViewEvents.CardDatabaseUpdated>(
                    __ => LoadCards()));

            void LoadCards()
            {
                IEnumerable<CardInfo> cardInfos = _cardDatabase.GetCards().Result;

                _cardInfoByDatabaseID = cardInfos.ToDictionary(__cardInfo => __cardInfo.DatabaseID);
                _cardInfoByID = cardInfos.ToDictionary(__cardInfo => __cardInfo.ID);
            }
        }

        CardInfo? ICardInfoProvider.GetCardInfo(
            string cardID)
        {
            if (_cardInfoByID.TryGetValue(cardID, out CardInfo cardInfo))
                return cardInfo;

            return null;
        }

        CardInfo? ICardInfoProvider.GetCardInfo(
            int databaseID)
        {
            if (_cardInfoByDatabaseID.TryGetValue(databaseID, out CardInfo cardInfo))
                return cardInfo;

            return null;
        }
    }
}
