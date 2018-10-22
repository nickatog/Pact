using System;

namespace Pact
{
    [Serializable]
    public struct GameResult
    {
        public bool GameWon { get; private set; }
        public string OpponentClass { get; private set; }
        public DateTime Timestamp { get; private set; }

        public GameResult(
            DateTime timestamp,
            bool gameWon,
            string opponentClass)
        {
            GameWon = gameWon;
            OpponentClass = opponentClass;
            Timestamp = timestamp;
        }
    }
}
