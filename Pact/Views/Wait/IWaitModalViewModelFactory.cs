using System;

namespace Pact
{
    public interface IWaitModalViewModelFactory
    {
        WaitModalViewModel Create(
            Action @delegate);
    }
}
