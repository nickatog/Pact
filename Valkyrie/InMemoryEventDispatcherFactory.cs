namespace Valkyrie
{
    public sealed class InMemoryEventDispatcherFactory
        : IEventDispatcherFactory
    {
        IEventDispatcher IEventDispatcherFactory.Create()
        {
            return new InMemoryEventDispatcher();
        }
    }
}
