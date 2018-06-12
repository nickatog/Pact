using System.Threading.Tasks;

namespace Pact
{
    public interface IConfigurationStorage
    {
        Task SaveChanges(
            ConfigurationData configurationData);
    }
}
