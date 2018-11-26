namespace Pact.GameEvents
{
    public sealed class CardReturnedToDeckFromHand
    {
        public CardReturnedToDeckFromHand(
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
