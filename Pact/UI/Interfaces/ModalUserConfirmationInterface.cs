using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalUserConfirmationInterface
        : IUserConfirmationInterface
    {
        #region Private members
        private readonly IModalDisplay _modalDisplay;
        #endregion // Private members

        public ModalUserConfirmationInterface(
            #region Dependency assignments
            IModalDisplay modalDisplay)
        {
            _modalDisplay =
                modalDisplay.Require(nameof(modalDisplay));
            #endregion // Dependency assignments
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
