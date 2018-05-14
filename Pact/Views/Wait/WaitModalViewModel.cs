using System;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class WaitModalViewModel
        : IModalViewModel<bool>
    {
        private readonly Task _finished;

        public WaitModalViewModel(
            Action @delegate)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            _finished = taskCompletionSource.Task;

            Task.Run(@delegate)
            .ContinueWith(
                __ => taskCompletionSource.SetResult(true));
        }

        event Action<bool> IModalViewModel<bool>.OnClosed
        {
            add
            {
                _finished.ContinueWith(__ => value?.Invoke(true));
            }
            remove
            { }
        }
    }
}
