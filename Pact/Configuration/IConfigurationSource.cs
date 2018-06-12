using System.Threading.Tasks;

namespace Pact
{
    public interface IConfigurationSource
    {
        Task<IConfigurationSettings> GetSettings();
    }
}
