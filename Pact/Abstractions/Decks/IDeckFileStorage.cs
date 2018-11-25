using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckFileStorage
    {
        Task<IEnumerable<Models.Data.Deck>> GetAll();

        Task SaveAll(
            IEnumerable<Models.Data.Deck> decks);
    }
}
