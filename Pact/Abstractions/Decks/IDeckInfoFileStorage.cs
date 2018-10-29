using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckInfoFileStorage
    {
        Task<IEnumerable<DeckInfo>> GetAll();

        Task SaveAll(
            IEnumerable<DeckInfo> deckInfos);
    }
}
