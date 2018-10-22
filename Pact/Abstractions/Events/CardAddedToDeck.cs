namespace Pact.Events
{
    public sealed class CardAddedToDeck
    {
        public string CardID { get; private set; }
        public int PlayerID { get; private set; }

        public CardAddedToDeck(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }
    }
}
