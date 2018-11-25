using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckRepository
    {
        Task<IEnumerable<Models.Client.Deck>> GetAllDecks();

        Task ReplaceAllDecks(
            IEnumerable<Models.Client.DeckDetail> deckDetails);

        Task UpdateDeck(
            Models.Client.DeckDetail deckDetail);
    }
}
