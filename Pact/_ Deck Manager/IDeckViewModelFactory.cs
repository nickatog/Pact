using System;
using System.Collections.Generic;

namespace Pact
{
    public interface IDeckViewModelFactory
    {
        DeckViewModel Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Guid deckID,
            Decklist decklist,
            IEnumerable<GameResult> gameResults = null);
    }
}
