using System;

namespace Pact.Events
{
    public sealed class DeckDeleted
    {
        public DeckDeleted(
            Guid deckID)
        {
            DeckID = deckID;
        }

        public Guid DeckID { get; private set; }
    }
}
