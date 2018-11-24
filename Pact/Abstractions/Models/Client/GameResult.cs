using System;

namespace Pact.Models.Client
{
    public struct GameResult
    {
        public GameResult(
            DateTime timestamp,
            bool gameWon,
            string opponentClass)
        {
            Timestamp = timestamp;
            GameWon = gameWon;
            OpponentClass = opponentClass;
        }

        public bool GameWon { get; }
        public string OpponentClass { get; }
        public DateTime Timestamp { get; }
    }
}
