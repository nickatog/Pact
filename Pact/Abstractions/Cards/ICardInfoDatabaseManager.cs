using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardInfoDatabaseManager
    {
        int? GetCurrentVersion();

        Task UpdateCardInfoDatabase(
            int version,
            Stream updateStream);
    }
}
