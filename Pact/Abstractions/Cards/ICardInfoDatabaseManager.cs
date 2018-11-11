using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public interface ICardInfoDatabaseManager
    {
        int? GetCurrentVersion();

        // how to update existing file? can it be done in-place? (maybe, since the JSON provider loads the entire thing?)
        Task UpdateCardInfoDatabase(
            Stream updateStream);
    }

    public sealed class DummyCardInfoDatabaseManager
        : ICardInfoDatabaseManager
    {
        int? ICardInfoDatabaseManager.GetCurrentVersion()
        {
            return 10101;
        }

        Task ICardInfoDatabaseManager.UpdateCardInfoDatabase(Stream updateStream)
        {
            throw new System.NotImplementedException();
        }
    }
}
