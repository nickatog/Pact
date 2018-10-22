using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalUserConfirmationInterface
        : IUserConfirmationInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IUserConfirmationModalViewModelFactory _viewModelFactory;

        public ModalUserConfirmationInterface(
            IModalDisplay modalDisplay,
            IUserConfirmationModalViewModelFactory viewModelFactory)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _viewModelFactory = viewModelFactory.Require(nameof(viewModelFactory));
        }

        Task<bool> IUserConfirmationInterface.Confirm(
            string messageText,
            string acceptText,
            string declineText)
        {
            var result = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                _viewModelFactory.Create(messageText, acceptText, declineText),
                __result => result.SetResult(__result));

            return result.Task;
        }
    }
}
