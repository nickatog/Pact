using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardInfoDatabaseUpdateService
    {
        Task<int?> GetLatestVersion();

        Task<Stream> GetLatestVersionStream();
    }
}
