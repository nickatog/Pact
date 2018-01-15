using System.Collections.Generic;

namespace Pact
{
    public sealed class ParseContext
    {
        public string CoinEntityID { get; set; }

        public string CurrentGameStep { get; set; }

        private readonly IDictionary<string, string> _entityMappings = new Dictionary<string, string>();
        public IDictionary<string, string> EntityMappings => _entityMappings;

        private readonly IList<string> _gameLosers = new List<string>();
        public IList<string> GameLosers => _gameLosers;

        private readonly IList<string> _gameWinners = new List<string>();
        public IList<string> GameWinners => _gameWinners;

        public BlockContext ParentBlock { get; set; }

        public IEnumerable<IGameStateDebugEventParser> Parsers { get; private set; }

        private readonly IDictionary<string, string> _playerHeroCards = new Dictionary<string, string>();
        public IDictionary<string, string> PlayerHeroCards => _playerHeroCards;

        public ParseContext(
            IEnumerable<IGameStateDebugEventParser> parsers)
        {
            Parsers = parsers;
        }

        public void Reset()
        {
            CoinEntityID = null;
            CurrentGameStep = null;
            _entityMappings.Clear();
            _gameLosers.Clear();
            _gameWinners.Clear();
        }
    }
}
