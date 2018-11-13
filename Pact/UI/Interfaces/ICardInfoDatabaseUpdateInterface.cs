using System.Threading.Tasks;

namespace Pact
{
    public interface ICardInfoDatabaseUpdateInterface
    {
        Task CheckForUpdates();
    }
}
