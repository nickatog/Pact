using System.Collections.Generic;
using System.Linq;

namespace Pact.Events
{
    public sealed class GameEnded
    {
        public IEnumerable<string> HeroCardIDs { get; private set; }
        public IEnumerable<string> Losers { get; private set; }
        public IEnumerable<string> Winners { get; private set; }

        public GameEnded(
            IEnumerable<string> winners,
            IEnumerable<string> losers,
            IEnumerable<string> heroCardIDs)
        {
            HeroCardIDs = heroCardIDs.ToList();
            Losers = losers.ToList();
            Winners = winners.ToList();
        }
    }
}
