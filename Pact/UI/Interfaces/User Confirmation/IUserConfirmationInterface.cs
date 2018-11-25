using System.Threading.Tasks;

namespace Pact
{
    public interface IUserConfirmationInterface
    {
        Task<bool> Confirm(
            string messageText,
            string acceptText,
            string declineText);
    }
}
