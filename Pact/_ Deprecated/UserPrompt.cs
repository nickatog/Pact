using System;
using System.Windows.Threading;

namespace Pact
{
    public sealed class UserPrompt
        : IUserPrompt
    {
        private readonly Dispatcher _dispatcher;

        public UserPrompt(
            Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        bool IUserPrompt.Display(
            string message,
            string confirmText,
            string cancelText)
        {
            return new UserPromptView(message, confirmText, cancelText).ShowDialog() ?? false;
        }
    }
}
