using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IWaitInterface
    {
        Task Perform(
            Action @delegate);
    }
}
