using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class AsyncSemaphore
    {
        private int _count;
        private readonly Queue<TaskCompletionSource<IDisposable>> _waiters = new Queue<TaskCompletionSource<IDisposable>>();

        public AsyncSemaphore(
            int count = 1)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Must be greater than zero!");

            _count = count;
        }

        public Task<IDisposable> WaitAsync()
        {
            lock (_waiters)
            {
                if (_count > 0)
                {
                    _count--;

                    return Task.FromResult<IDisposable>(new WaitHandle(this));
                }

                var waiter = new TaskCompletionSource<IDisposable>();

                _waiters.Enqueue(waiter);

                return waiter.Task;
            }
        }

        private void Release()
        {
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                {
                    _waiters.Dequeue().SetResult(new WaitHandle(this));

                    return;
                }

                _count++;
            }
        }

        private sealed class WaitHandle
            : IDisposable
        {
            private bool _disposed = false;
            private readonly AsyncSemaphore _parent;

            public WaitHandle(
                AsyncSemaphore parent)
            {
                _parent = parent;
            }

            void Dispose(
                bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                        _parent.Release();

                    _disposed = true;
                }
            }

            void IDisposable.Dispose()
            {
                Dispose(true);
            }
        }
    }
}
