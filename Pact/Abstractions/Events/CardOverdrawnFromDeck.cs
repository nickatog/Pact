namespace Pact.Events
{
    public sealed class CardOverdrawnFromDeck
    {
        public string CardID { get; private set; }
        public int PlayerID { get; private set; }

        public CardOverdrawnFromDeck(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }
    }
}
