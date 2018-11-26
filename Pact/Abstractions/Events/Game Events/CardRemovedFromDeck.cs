namespace Pact.GameEvents
{
    public sealed class CardRemovedFromDeck
    {
        public CardRemovedFromDeck(
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
