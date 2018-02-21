using System;
using System.Collections.Generic;

namespace Pact
{
    public interface IDeckViewModelFactory
    {
        DeckViewModel Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Action<DeckViewModel> moveUp,
            Action<DeckViewModel> moveDown,
            Action<DeckViewModel> delete,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults = null);
    }
}
