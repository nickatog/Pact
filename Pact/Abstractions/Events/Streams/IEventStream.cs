using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pact
{
    public interface IEventStream
        : IDisposable
    {
        Task<object> ReadNext(
            CancellationToken? cancellationToken = null);

        void SeekEnd();
    }
}
