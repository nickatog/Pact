using System;

namespace Pact.Commands
{
    public sealed class DeleteDeck
    {
        public DeleteDeck(
            Guid deckID)
        {
            DeckID = deckID;
        }

        public Guid DeckID { get; private set; }
    }
}
