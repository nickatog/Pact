using Valkyrie;

namespace Pact
{
    public interface IPlayerDeckTrackerViewModelFactory
    {
        PlayerDeckTrackerViewModel Create(
            IEventDispatcher gameEventDispatcher,
            Decklist decklist);
    }
}
