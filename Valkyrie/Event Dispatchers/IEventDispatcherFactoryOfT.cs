namespace Valkyrie
{
    public interface IEventDispatcherFactory<T>
    {
        IEventDispatcher<T> Create();
    }
}
