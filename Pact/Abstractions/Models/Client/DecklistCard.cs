namespace Pact.Models.Client
{
    public struct DecklistCard
    {
        public DecklistCard(
            string cardID,
            int count)
        {
            CardID = cardID;
            Count = count;
        }

        public string CardID { get; }
        public int Count { get; }
    }
}
