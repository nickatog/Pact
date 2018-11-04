using System;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class UserConfirmationModalViewModel
        : IModalViewModel<bool>
    {
        public UserConfirmationModalViewModel(
            #region Dependency assignments
            string messageText,
            string acceptText,
            string declineText)
        {
            MessageText =
                messageText.Require(nameof(messageText));

            AcceptText =
                acceptText.Require(nameof(acceptText));

            DeclineText =
                declineText.Require(nameof(declineText));
            #endregion // Dependency assignments
        }

        public event Action<bool> OnClosed;

        public ICommand Accept =>
            new DelegateCommand(
                () => OnClosed?.Invoke(true));

        public string AcceptText { get; }

        public ICommand Decline =>
            new DelegateCommand(
                () => OnClosed?.Invoke(false));

        public string DeclineText { get; }

        public string MessageText { get; }
    }
}
