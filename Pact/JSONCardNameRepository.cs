using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Pact
{
    public sealed class JSONCardNameRepository
        : ICardNameRepository
    {
        private readonly IDictionary<string, string> _cardNameByID;

        public JSONCardNameRepository(
            string filePath)
        {
            if (filePath == null || !File.Exists(filePath))
                throw new ArgumentException("Path must not be null and the card file must exist!", nameof(filePath));

            _cardNameByID =
                JsonConvert.DeserializeObject<Card[]>(File.ReadAllText(filePath))
                .ToDictionary(__card => __card.id, __card => __card.name);
        }

        string ICardNameRepository.GetCardName(
            string cardID)
        {
            if (_cardNameByID.TryGetValue(cardID, out string cardName))
                return cardName;

            return null;
        }

        private struct Card
        {
            public string id;
            public string name;
        }
    }
}
