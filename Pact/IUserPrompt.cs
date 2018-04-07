namespace Pact
{
    public interface IUserPrompt
    {
        bool Display(
            string message,
            string confirmText,
            string cancelText);
    }
}
