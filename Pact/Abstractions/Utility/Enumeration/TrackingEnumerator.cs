using System;
using System.Collections;
using System.Collections.Generic;

namespace Pact
{
    public sealed class TrackingEnumerator<T>
        : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public TrackingEnumerator(
            IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public T Current => _enumerator.Current;

        object IEnumerator.Current => _enumerator.Current;

        void IDisposable.Dispose()
        {
            _enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return !(HasCompleted = !_enumerator.MoveNext());
        }

        public void Reset()
        {
            _enumerator.Reset();

            HasCompleted = false;
        }

        public bool HasCompleted { get; private set; }
    }
}
