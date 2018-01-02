using System.Threading.Tasks;

namespace Pact
{
    public interface IEventStream
    {
        Task<object> ReadNext();
    }
}
