using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardDatabaseManager
    {
        int? GetCurrentVersion();

        Task UpdateCardDatabase(
            int version,
            Stream updateStream);
    }
}
