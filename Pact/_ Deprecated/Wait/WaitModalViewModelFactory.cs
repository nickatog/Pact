using System;

namespace Pact
{
    public sealed class WaitModalViewModelFactory
        : IWaitModalViewModelFactory
    {
        WaitModalViewModel IWaitModalViewModelFactory.Create(
            Action @delegate)
        {
            return new WaitModalViewModel(@delegate);
        }
    }
}
