namespace Pact.Events
{
    public sealed class CardEnteredPlayFromDeck
    {
        public string CardID { get; private set; }
        public int PlayerID { get; private set; }

        public CardEnteredPlayFromDeck(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }
    }
}
