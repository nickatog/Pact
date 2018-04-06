using System;

namespace Pact
{
    public sealed class UserPrompt
        : IUserPrompt
    {
        void IUserPrompt.Display(
            string message,
            string confirmText,
            Action continuation,
            string cancelText)
        {
            if (new UserPromptView(message, confirmText, cancelText).ShowDialog() ?? false)
                continuation?.Invoke();
        }
    }
}
