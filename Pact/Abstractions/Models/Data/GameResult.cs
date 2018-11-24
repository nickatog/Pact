using System;

namespace Pact.Models.Data
{
    public struct GameResult
    {
        public bool GameWon;
        public string OpponentClass;
        public DateTime Timestamp;

        public GameResult(
            DateTime timestamp,
            bool gameWon,
            string opponentClass)
        {
            Timestamp = timestamp;
            GameWon = gameWon;
            OpponentClass = opponentClass;
        }
    }
}
