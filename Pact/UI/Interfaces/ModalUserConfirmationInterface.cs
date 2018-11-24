using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalUserConfirmationInterface
        : IUserConfirmationInterface
    {
        private readonly IModalDisplay _modalDisplay;

        public ModalUserConfirmationInterface(
            IModalDisplay modalDisplay)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task<bool> IUserConfirmationInterface.Confirm(
            string messageText,
            string acceptText,
            string declineText)
        {
            var completionSource = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                new UserConfirmationModalViewModel(messageText, acceptText, declineText),
                __result => completionSource.SetResult(__result));

            return completionSource.Task;
        }
    }
}
