namespace Pact.Events
{
    public sealed class DeckAdded
    {
        public DeckAdded(
            DeckViewModel deckViewModel)
        {
            DeckViewModel = deckViewModel;
        }

        public DeckViewModel DeckViewModel { get; }
    }
}
