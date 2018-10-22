using System;
using System.Threading.Tasks;

namespace Pact
{
    internal sealed class BackgroundWorkModalViewModelFactory
        : IBackgroundWorkModalViewModelFactory
    {
        IModalViewModel<bool> IBackgroundWorkModalViewModelFactory.Create(
            Func<Action<string>, Task> @delegate)
        {
            return new BackgroundWorkModalViewModel(@delegate);
        }
    }
}
