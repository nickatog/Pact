using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IGameResultRepository
    {
        Task AddGameResult(
            Guid deckID,
            GameResult gameResult);
    }
}
