using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardDatabase
    {
        Task<IEnumerable<CardInfo>> GetCards();
    }
}
