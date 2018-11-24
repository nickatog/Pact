using System;
using System.Collections.Generic;
using System.Linq;

namespace Pact.Models.Data
{
    public struct Deck
    {
        public Guid DeckID;
        public string DeckString;
        public IEnumerable<GameResult> GameResults;
        public UInt16 Position;
        public string Title;

        public Deck(
            Guid deckID,
            string deckString,
            string title,
            UInt16 position,
            IEnumerable<GameResult> gameResults)
        {
            DeckID = deckID;
            DeckString = deckString;
            GameResults = gameResults?.ToList();
            Position = position;
            Title = title;
        }
    }
}
