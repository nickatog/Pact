namespace Pact.Events
{
    public sealed class CardRemovedFromDeck
    {
        public string CardID { get; private set; }
        public int PlayerID { get; private set; }

        public CardRemovedFromDeck(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }
    }
}
