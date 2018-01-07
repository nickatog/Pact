namespace Valkyrie
{
    public sealed class InMemoryEventDispatcherFactory<T>
        : IEventDispatcherFactory<T>
    {
        IEventDispatcher<T> IEventDispatcherFactory<T>.Create()
        {
            return new InMemoryEventDispatcher<T>();
        }
    }
}
