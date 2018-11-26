namespace Pact.GameEvents
{
    public sealed class CardEnteredPlayFromDeck
    {
        public CardEnteredPlayFromDeck(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }

        public string CardID { get; }
        public int PlayerID { get; }
    }
}
