namespace Pact.Events
{
    public sealed class GameEnded
    {
        public bool GameWon { get; private set; }
        public string OpponentHeroCardID { get; private set; }

        public GameEnded(
            bool gameWon,
            string opponentHeroCardID)
        {
            GameWon = gameWon;
            OpponentHeroCardID = opponentHeroCardID;
        }
    }
}
