using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONCardInfoProvider
        : ICardInfoProvider
    {
        #region Private members
        private IDictionary<int, Card> _cardByDatabaseID;
        private IDictionary<string, Card> _cardByID;
        private readonly string _filePath;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Private members

        public JSONCardInfoProvider(
            #region Dependency assignments
            string filePath,
            IEventDispatcher viewEventDispatcher)
        {
            if (filePath == null || !File.Exists(filePath))
                throw new ArgumentException("Path must not be null and the card file must exist!", nameof(filePath));

            _filePath = filePath;

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));
            #endregion // Dependency assignments

            LoadCards();

            _viewEventDispatcher.RegisterHandler(
                new DelegateEventHandler<Events.CardDatabaseUpdated>(
                    __ => LoadCards()));

            void LoadCards()
            {
                Card[] cards = JsonConvert.DeserializeObject<Card[]>(File.ReadAllText(_filePath));

                _cardByDatabaseID = cards.ToDictionary(__card => __card.dbfId, __card => __card);
                _cardByID = cards.ToDictionary(__card => __card.id, __card => __card);
            }
        }

        CardInfo? ICardInfoProvider.GetCardInfo(
            string cardID)
        {
            if (_cardByID.TryGetValue(cardID, out Card card))
                return new CardInfo(card.name, card.cardClass, card.cost, card.id, card.dbfId);

            return null;
        }

        CardInfo? ICardInfoProvider.GetCardInfo(
            int databaseID)
        {
            if (_cardByDatabaseID.TryGetValue(databaseID, out Card card))
                return new CardInfo(card.name, card.cardClass, card.cost, card.id, card.dbfId);

            return null;
        }

        private struct Card
        {
            public string cardClass;
            public int cost;
            public int dbfId;
            public string id;
            public string name;
        }
    }
}
