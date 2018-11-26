namespace Pact.GameEvents
{
    public sealed class MulliganOptionPresented
    {
        public MulliganOptionPresented(
            string cardID)
        {
            CardID = cardID;
        }

        public string CardID { get; }
    }
}
