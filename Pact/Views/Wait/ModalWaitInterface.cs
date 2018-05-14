using System;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalWaitInterface
        : IWaitInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IWaitModalViewModelFactory _waitModalViewModelFactory;

        public ModalWaitInterface(
            IModalDisplay modalDisplay,
            IWaitModalViewModelFactory waitModalViewModelFactory)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _waitModalViewModelFactory = waitModalViewModelFactory.Require(nameof(waitModalViewModelFactory));
        }

        Task IWaitInterface.Perform(
            Action @delegate)
        {
            var result = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                _waitModalViewModelFactory.Create(@delegate),
                __ => result.SetResult(true));

            return result.Task;
        }
    }
}
