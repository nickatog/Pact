using System.Threading.Tasks;

namespace Pact
{
    public interface IReplaceDeckInterface
    {
        Task<Models.Client.Decklist?> GetDecklist();
    }
}
