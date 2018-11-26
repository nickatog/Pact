namespace Pact.GameEvents
{
    public sealed class CardDrawnFromDeck
    {
        public CardDrawnFromDeck(
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
