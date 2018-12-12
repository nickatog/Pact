using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalAboutPageInterface
        : IAboutPageInterface
    {
        private readonly IModalDisplay _modalDisplay;

        public ModalAboutPageInterface(
            IModalDisplay modalDisplay)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task IAboutPageInterface.Show()
        {
            var completionSource = new TaskCompletionSource<object>();

            _modalDisplay.Show(
                new AboutPageModalViewModel(),
                __ => completionSource.SetResult(null));

            return completionSource.Task;
        }
    }
}
