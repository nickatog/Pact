namespace Pact.GameEvents
{
    public sealed class CardOverdrawnFromDeck
    {
        public CardOverdrawnFromDeck(
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
