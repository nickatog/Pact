using System.Collections.Generic;

namespace Pact
{
    public sealed class ParseContext
    {
        public ParseContext(
            IEnumerable<IGameStateDebugEventParser> parsers)
        {
            Parsers = parsers;

            EntityMappings = new Dictionary<string, string>();
            GameLosers = new List<string>();
            GameWinners = new List<string>();
            PlayerHeroCards = new Dictionary<string, string>();
            PlayerNames = new Dictionary<string, string>();
        }

        public string CoinEntityID { get; set; }
        public string CurrentGameStep { get; set; }
        public IDictionary<string, string> EntityMappings { get; }
        public IList<string> GameLosers { get; }
        public IList<string> GameWinners { get; }
        public BlockContext ParentBlock { get; set; }
        public IEnumerable<IGameStateDebugEventParser> Parsers { get; }
        public IDictionary<string, string> PlayerHeroCards { get; }
        public string PlayerID { get; set; }
        public IDictionary<string, string> PlayerNames { get; }

        public void Reset()
        {
            CoinEntityID = null;
            CurrentGameStep = null;
            EntityMappings.Clear();
            GameLosers.Clear();
            GameWinners.Clear();
            PlayerHeroCards.Clear();
            PlayerID = null;
            PlayerNames.Clear();
        }
    }
}
