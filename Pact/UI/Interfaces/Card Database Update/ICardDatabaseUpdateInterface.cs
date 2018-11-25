using System.Threading.Tasks;

namespace Pact
{
    public interface ICardDatabaseUpdateInterface
    {
        Task CheckForUpdates();
    }
}
