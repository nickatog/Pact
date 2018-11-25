using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardDatabase
    {
        Task<IEnumerable<Models.Data.Card>> GetCards();
    }
}
