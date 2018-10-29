using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckRepository
    {
        Task<IEnumerable<DeckInfo>> GetAllDecksAndGameResults();

        Task ReplaceDecks(
            IEnumerable<DeckDetails> deckDetails);

        Task UpdateDeck(
            DeckDetails deckDetails);
    }
}
