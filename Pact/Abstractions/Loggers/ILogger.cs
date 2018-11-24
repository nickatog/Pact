using System.Threading.Tasks;

namespace Pact
{
    public interface ILogger
    {
        Task Write(
            string message);
    }
}
