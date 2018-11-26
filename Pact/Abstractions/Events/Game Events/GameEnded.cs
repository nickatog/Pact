namespace Pact.GameEvents
{
    public sealed class GameEnded
    {
        public GameEnded(
            bool gameWon,
            string opponentHeroCardID)
        {
            GameWon = gameWon;
            OpponentHeroCardID = opponentHeroCardID;
        }

        public bool GameWon { get; }
        public string OpponentHeroCardID { get; }
    }
}
