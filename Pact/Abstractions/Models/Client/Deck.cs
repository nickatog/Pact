using System;
using System.Collections.Generic;
using System.Linq;

namespace Pact.Models.Client
{
    public struct Deck
    {
        public Deck(
            Guid deckID,
            string deckString,
            string title,
            UInt16 position,
            IEnumerable<GameResult> gameResults)
        {
            DeckID = deckID;
            DeckString = deckString;
            GameResults = gameResults?.ToList() ?? Enumerable.Empty<GameResult>();
            Position = position;
            Title = title;
        }

        public Guid DeckID { get; }
        public string DeckString { get; }
        public IEnumerable<GameResult> GameResults { get; }
        public UInt16 Position { get; }
        public string Title { get; }
    }
}
