namespace Pact.Events
{
    public sealed class MulliganOptionPresented
    {
        public string CardID { get; private set; }

        public MulliganOptionPresented(
            string cardID)
        {
            CardID = cardID;
        }
    }
}
