namespace Pact
{
    public interface IPlayerDeckTrackerInterface
    {
        void Close();

        void TrackDeck(
            Models.Client.Decklist decklist);
    }
}
