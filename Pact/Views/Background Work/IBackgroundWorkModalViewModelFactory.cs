using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IBackgroundWorkModalViewModelFactory
    {
        BackgroundWorkModalViewModel Create(
            Func<Action<string>, Task> @delegate);
    }
}
