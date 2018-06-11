using System;
using System.Windows.Input;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class UserConfirmationModalViewModel
        : IModalViewModel<bool>
    {
        private readonly string _acceptText;
        private readonly string _declineText;
        private readonly string _messageText;

        public UserConfirmationModalViewModel(
            string messageText,
            string acceptText,
            string declineText)
        {
            _messageText = messageText.Require(nameof(messageText));
            _acceptText = acceptText.Require(nameof(acceptText));
            _declineText = declineText.Require(nameof(declineText));
        }

        public string AcceptText => _acceptText;

        public ICommand Accept =>
            new DelegateCommand(
                () => OnClosed?.Invoke(true));

        public string DeclineText => _declineText;

        public ICommand Decline =>
            new DelegateCommand(
                () => OnClosed?.Invoke(false));

        public string MessageText => _messageText;

        public event Action<bool> OnClosed;
    }
}
