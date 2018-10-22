namespace Pact.Events
{
    public sealed class CardReturnedToDeckFromHand
    {
        public string CardID { get; private set; }
        public int PlayerID { get; private set; }

        public CardReturnedToDeckFromHand(
            int playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }
    }
}
