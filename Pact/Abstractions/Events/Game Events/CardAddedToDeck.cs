namespace Pact.GameEvents
{
    public sealed class CardAddedToDeck
    {
        public CardAddedToDeck(
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
