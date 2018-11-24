using System;

namespace Pact.Models.Client
{
    public struct DeckDetail
    {
        public DeckDetail(
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
