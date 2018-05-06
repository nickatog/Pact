using System.Threading.Tasks;

namespace Pact
{
    public interface IUserConfirmation
    {
        Task<bool> Confirm(
            string message,
            string accept,
            string decline);
    }
}
