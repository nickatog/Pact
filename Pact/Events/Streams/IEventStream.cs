using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IEventStream
        : IDisposable
    {
        Task<object> ReadNext();

        // SkipToEnd()?
    }
}
