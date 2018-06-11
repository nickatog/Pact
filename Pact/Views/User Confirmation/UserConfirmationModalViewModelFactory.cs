namespace Pact
{
    public sealed class UserConfirmationModalViewModelFactory
        : IUserConfirmationModalViewModelFactory
    {
        IModalViewModel<bool> IUserConfirmationModalViewModelFactory.Create(
            string messageText,
            string acceptText,
            string declineText)
        {
            return new UserConfirmationModalViewModel(messageText, acceptText, declineText);
        }
    }
}
