using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface IDeckInfoRepository
    {
        Task<IEnumerable<DeckInfo>> GetAll();

        Task ReplaceAll(
            IEnumerable<DeckInfo> deckInfos);

        Task Save(
            DeckInfo deckInfo);
    }
}
