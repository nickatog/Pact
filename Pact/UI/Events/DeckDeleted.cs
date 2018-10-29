namespace Pact.Events
{
    public sealed class DeckDeleted
    {
        public DeckDeleted(
            DeckViewModel deckViewModel)
        {
            DeckViewModel = deckViewModel;
        }

        public DeckViewModel DeckViewModel { get; }
    }
}
