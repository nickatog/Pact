using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class VarintDecklistSerializer
        : IDecklistSerializer
    {
        private readonly ICardInfoProvider _cardInfoProvider;

        public VarintDecklistSerializer(
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
        }

        async Task<Decklist> IDecklistSerializer.Deserialize(Stream stream)
        {
            stream.Require(nameof(stream));

            stream.Seek(3, SeekOrigin.Begin);

            (int heroDatabaseID, _) = (await __ParseCards(1)).FirstOrDefault();
            if (heroDatabaseID == 0)
                throw new Exception("Failed to parse hero!");

            CardInfo? heroInfo = _cardInfoProvider.GetCardInfo(heroDatabaseID);
            if (heroInfo == null)
                throw new Exception($"Could not resolve hero ({heroDatabaseID})!");

            IEnumerable<(string, int)> cards =
                (await __ParseCards(1))
                    .Concat(
                (await __ParseCards(2))
                    .Concat(
                (await __ParseCards())))
                .Select(
                    __cardData =>
                    {
                        CardInfo? cardInfo = _cardInfoProvider.GetCardInfo(__cardData.DatabaseID);
                        if (cardInfo == null)
                            throw new Exception($"Could not resolve card ({__cardData.DatabaseID})!");

                        return (cardInfo.Value.ID, __cardData.Count);
                    });

            return new Decklist(heroInfo.Value.ID, cards);

            async Task<IEnumerable<(int DatabaseID, int Count)>> __ParseCards(int? count = null)
            {
                int numCards = await Varint.Parse(stream);
                var parsedCards = new (int, int)[numCards];
                for (int index = 0; index < numCards; index++)
                    parsedCards[index] = (await Varint.Parse(stream), count ?? await Varint.Parse(stream));

                return parsedCards;
            }
        }

        async Task IDecklistSerializer.Serialize(Stream stream, Decklist decklist)
        {
            stream.Require(nameof(stream));

            IEnumerable<(string CardID, int Count)> decklistCards = decklist.Cards;
            decklistCards.Require($"{nameof(decklist)}.{nameof(decklist.Cards)}");

            await stream.WriteAsync(new byte[] { 0, 1, 2 }, 0, 3);

            await __WriteCardsToStream(new List<(string, int)> { (decklist.HeroID, 1) });

            await __WriteCardsToStream(decklistCards.Where(__card => __card.Count == 1));

            await __WriteCardsToStream(decklistCards.Where(__card => __card.Count == 2));

            await __WriteCardsToStream(decklistCards.Where(__card => __card.Count > 2), true);

            async Task __WriteCardsToStream(IEnumerable<(string CardID, int Count)> cards, bool includeCount = false)
            {
                cards = cards.ToList();

                byte[] buffer =
                    Varint.GetBytes(cards.Count())
                        .Concat(
                    cards.SelectMany(
                        __card =>
                        {
                            int? databaseID = _cardInfoProvider.GetCardInfo(__card.CardID)?.DatabaseID;
                            if (!databaseID.HasValue)
                                throw new Exception($"Could not resolve card ({__card.CardID})!");

                            return
                                Varint.GetBytes(databaseID.Value)
                                    .Concat(
                                includeCount ? Varint.GetBytes(__card.Count) : Enumerable.Empty<byte>());
                        }))
                    .ToArray();

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
