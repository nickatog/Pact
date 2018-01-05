namespace Pact.Events
{
    public sealed class CardEnteredPlayFromDeck
    {
        public string CardID { get; private set; }
        public string PlayerID { get; private set; }

        public CardEnteredPlayFromDeck(
            string playerID,
            string cardID)
        {
            CardID = cardID;
            PlayerID = playerID;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "{0} ({1}: {2}, {3}: {4})",
                    nameof(CardEnteredPlayFromDeck),
                    nameof(PlayerID),
                    PlayerID,
                    nameof(CardID),
                    CardID);
        }
    }
}
