using System.Threading.Tasks;

namespace Pact
{
    public interface IReplaceDeckInterface
    {
        Task<Decklist?> GetReplacementDecklist();
    }
}
