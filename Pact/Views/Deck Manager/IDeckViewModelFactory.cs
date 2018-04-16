using System;
using System.Collections.Generic;

namespace Pact
{
    public interface IDeckViewModelFactory
    {
        DeckViewModel Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Action<DeckViewModel, int> emplaceDeck,
            Func<DeckViewModel, int> findPosition,
            Action<DeckViewModel> delete,
            Action<DeckViewModel> saveDeck,
            Guid deckID,
            Decklist decklist,
            string title,
            IEnumerable<GameResult> gameResults = null);
    }
}
