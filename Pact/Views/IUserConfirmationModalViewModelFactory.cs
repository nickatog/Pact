namespace Pact
{
    public interface IUserConfirmationModalViewModelFactory
    {
        IModalViewModel<bool> Create(
            string messageText,
            string acceptText,
            string declineText);
    }
}
