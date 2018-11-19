using System;

namespace Pact.ViewCommands
{
    public sealed class DeleteDeck
    {
        public DeleteDeck(
            Guid deckID)
        {
            DeckID = deckID;
        }

        public Guid DeckID { get; }
    }
}
