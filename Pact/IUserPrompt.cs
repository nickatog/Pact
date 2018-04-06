using System;

namespace Pact
{
    public interface IUserPrompt
    {
        void Display(
            string message,
            string confirmText,
            Action continuation,
            string cancelText);
    }
}
