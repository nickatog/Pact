namespace Pact.Events
{
    public sealed class DeckTracking
    {
        public DeckTracking(
            DeckViewModel deckViewModel)
        {
            DeckViewModel = deckViewModel;
        }

        public DeckViewModel DeckViewModel { get; }
    }
}
