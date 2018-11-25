using System;

namespace Pact.Models.Client
{
    public struct GameResult
    {
        public GameResult(
            DateTimeOffset timestamp,
            bool gameWon,
            string opponentClass)
        {
            Timestamp = timestamp;
            GameWon = gameWon;
            OpponentClass = opponentClass;
        }

        public bool GameWon { get; }
        public string OpponentClass { get; }
        public DateTimeOffset Timestamp { get; }
    }
}
