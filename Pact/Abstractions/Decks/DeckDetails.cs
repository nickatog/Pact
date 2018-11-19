using System;

namespace Pact
{
    public struct DeckDetails
    {
        public DeckDetails(
            Guid deckID,
            string title,
            string deckString,
            UInt16 position)
        {
            DeckID = deckID;
            DeckString = deckString;
            Position = position;
            Title = title;
        }

        public Guid DeckID { get; }

        public string DeckString { get; }

        public UInt16 Position { get; }

        public string Title { get; }
    }
}
