using Valkyrie;

namespace Pact
{
    public interface ITrackedCardViewModelFactory
    {
        TrackedCardViewModel Create(
            IEventDispatcher gameEventDispatcher,
            string cardID,
            int count,
            int? playerID = null);
    }
}
