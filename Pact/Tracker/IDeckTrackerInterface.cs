namespace Pact
{
    public interface IDeckTrackerInterface
    {
        void Close();

        void StartTracking(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Decklist decklist);
    }
}
