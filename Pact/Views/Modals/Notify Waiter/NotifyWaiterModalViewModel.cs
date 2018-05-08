using System;

namespace Pact
{
    public sealed class NotifyWaiterModalViewModel
        : IModalViewModel<bool>
    {
        // take delegate and start on new thread?

        public event Action<bool> OnClosed;
    }
}
