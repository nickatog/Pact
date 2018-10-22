using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IBackgroundWorkModalViewModelFactory
    {
        IModalViewModel<bool> Create(
            Func<Action<string>, Task> @delegate);
    }
}
