namespace Pact
{
    public interface IDeckTrackerInterface
    {
        void Close();

        void StartTracking(
            Decklist decklist);
    }
}
