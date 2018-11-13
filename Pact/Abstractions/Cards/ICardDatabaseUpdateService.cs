using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardDatabaseUpdateService
    {
        Task<int?> GetLatestVersion();

        Task<Stream> GetVersionStream(
            int version);
    }
}
