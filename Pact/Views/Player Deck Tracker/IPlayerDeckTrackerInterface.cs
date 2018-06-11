namespace Pact
{
    public interface IPlayerDeckTrackerInterface
    {
        void Close();

        void TrackDeck(
            Decklist decklist);
    }
}
