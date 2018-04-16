using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckInfoRepository
    {
        Task<IEnumerable<DeckInfo>> GetAll();
        Task Save(DeckInfo deckInfo);
        Task Save(IEnumerable<DeckInfo> deckInfos);
    }
}
