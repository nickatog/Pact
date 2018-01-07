using System.Collections.Generic;

namespace Pact.Events
{
    public sealed class GameStarted
    {
        public IEnumerable<(int PlayerID, int HeroEntityID)> Players { get; private set; }

        public GameStarted(
            IEnumerable<(int, int)> players)
        {
            Players = players;
        }
    }
}
