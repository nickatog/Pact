using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IGameResultStorage
    {
        Task SaveGameResult(Guid deckID, GameResult gameResult);
    }
}
