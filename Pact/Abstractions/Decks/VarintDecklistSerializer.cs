using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class VarintDecklistSerializer
        : ISerializer<Decklist>
    {
        private readonly ICardInfoProvider _cardInfoProvider;

        public VarintDecklistSerializer(
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
        }

        Task<Decklist> ISerializer<Decklist>.Deserialize(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return __Deserialize();

            async Task<Decklist> __Deserialize()
            {
                await stream.ReadAsync(new byte[3], 0, 3).ConfigureAwait(false);

                (int heroDatabaseID, _) = (await __ParseCards(1).ConfigureAwait(false)).FirstOrDefault();
                if (heroDatabaseID == 0)
                    throw new Exception("Failed to parse hero!");

                string heroID = _cardInfoProvider.GetCardInfo(heroDatabaseID)?.ID;
                if (heroID == null)
                    throw new Exception($"Could not resolve hero ({heroDatabaseID})!");

                IEnumerable<DecklistCard> decklistCards =
                    (await __ParseCards(1).ConfigureAwait(false))
                        .Concat(
                    (await __ParseCards(2).ConfigureAwait(false))
                        .Concat(
                    (await __ParseCards().ConfigureAwait(false))))
                    .Select(
                        __cardData =>
                        {
                            string cardID = _cardInfoProvider.GetCardInfo(__cardData.DatabaseID)?.ID;
                            if (cardID == null)
                                throw new Exception($"Could not resolve card ({__cardData.DatabaseID})!");

                            return new DecklistCard(cardID, __cardData.Count);
                        });

                return new Decklist(heroID, decklistCards);
            }

            async Task<IEnumerable<(int DatabaseID, int Count)>> __ParseCards(
                int? count = null)
            {
                int numCards = await Varint.Parse(stream).ConfigureAwait(false);
                var parsedCards = new (int, int)[numCards];
                for (int index = 0; index < numCards; index++)
                    parsedCards[index] = (
                        await Varint.Parse(stream).ConfigureAwait(false),
                        count ?? await Varint.Parse(stream).ConfigureAwait(false));

                return parsedCards;
            }
        }

        Task ISerializer<Decklist>.Serialize(
            Stream stream,
            Decklist decklist)
        {
            stream.Require(nameof(stream));

            IEnumerable<DecklistCard> decklistCards = decklist.Cards;
            decklistCards.Require($"{nameof(decklist)}.{nameof(decklist.Cards)}");

            return __Serialize();

            async Task __Serialize()
            {
                await stream.WriteAsync(new byte[] { 0, 1, 2 }, 0, 3).ConfigureAwait(false);

                await __WriteCardsToStream(new List<DecklistCard> { new DecklistCard(decklist.HeroID, 1) }).ConfigureAwait(false);

                await __WriteCardsToStream(decklistCards.Where(__card => __card.Count == 1)).ConfigureAwait(false);

                await __WriteCardsToStream(decklistCards.Where(__card => __card.Count == 2)).ConfigureAwait(false);

                await __WriteCardsToStream(decklistCards.Where(__card => __card.Count > 2), true).ConfigureAwait(false);
            }

            async Task __WriteCardsToStream(
                IEnumerable<DecklistCard> cards,
                bool includeCount = false)
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

                await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
        }
    }
}
