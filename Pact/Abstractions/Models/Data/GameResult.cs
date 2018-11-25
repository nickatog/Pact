using System;

namespace Pact.Models.Data
{
    public struct GameResult
    {
        public bool GameWon;
        public string OpponentClass;
        public DateTimeOffset Timestamp;

        public GameResult(
            DateTimeOffset timestamp,
            bool gameWon,
            string opponentClass)
        {
            Timestamp = timestamp;
            GameWon = gameWon;
            OpponentClass = opponentClass;
        }
    }
}
