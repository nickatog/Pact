using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class AsyncSemaphore
    {
        private int _count;

        private readonly Queue<TaskCompletionSource<WaitHandle>> _waiters = new Queue<TaskCompletionSource<WaitHandle>>();

        public AsyncSemaphore(
            int count = 1)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Must be greater than zero!");

            _count = count;
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

        public Task<WaitHandle> WaitAsync()
        {
            lock (_waiters)
            {
                if (_count > 0)
                {
                    _count--;

                    return Task.FromResult(new WaitHandle(this));
                }

                var waiter = new TaskCompletionSource<WaitHandle>();
                _waiters.Enqueue(waiter);

                return waiter.Task;
            }
        }

        public sealed class WaitHandle
            : IDisposable
        {
            private readonly AsyncSemaphore _semaphore;

            public WaitHandle(
                AsyncSemaphore semaphore)
            {
                _semaphore = semaphore;
            }

            ~WaitHandle()
            {
                Dispose(false);
            }

            private bool _disposed = false;

            void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    _semaphore.Release();

                    _disposed = true;
                }
            }

            void IDisposable.Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }
        }
    }
}
