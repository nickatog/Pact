using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Pact
{
    public sealed class JSONCardInfoProvider
        : ICardInfoProvider
    {
        private readonly IDictionary<int, Card> _cardByDatabaseID;
        private readonly IDictionary<string, Card> _cardByID;

        // take view event dispatcher, refresh dictionaries when card info database is updated
        public JSONCardInfoProvider(
            string filePath)
        {
            if (filePath == null || !File.Exists(filePath))
                throw new ArgumentException("Path must not be null and the card file must exist!", nameof(filePath));

            Card[] cards = JsonConvert.DeserializeObject<Card[]>(File.ReadAllText(filePath));

            _cardByDatabaseID = cards.ToDictionary(__card => __card.dbfId, __card => __card);
            _cardByID = cards.ToDictionary(__card => __card.id, __card => __card);
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
