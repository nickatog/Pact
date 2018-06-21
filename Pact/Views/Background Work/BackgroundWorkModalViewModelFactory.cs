using System;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class BackgroundWorkModalViewModelFactory
        : IBackgroundWorkModalViewModelFactory
    {
        BackgroundWorkModalViewModel IBackgroundWorkModalViewModelFactory.Create(
            Func<Action<string>, Task> @delegate)
        {
            return new BackgroundWorkModalViewModel(@delegate);
        }
    }
}
