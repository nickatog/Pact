using System;
using System.Collections.Generic;

namespace Pact
{
    public interface IDeckViewModelFactory
    {
        DeckViewModel Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Func<DeckViewModel, int> findPosition,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults = null);
    }
}
