namespace Valkyrie
{
    public interface IEventDispatcher
    {
        void DispatchEvent(
            object @event);

        void RegisterHandler(
            IEventHandler handler);

        void UnregisterHandler(
            IEventHandler handler);
    }
}
