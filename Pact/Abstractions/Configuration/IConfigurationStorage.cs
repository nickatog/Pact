using System.Threading.Tasks;

namespace Pact
{
    public interface IConfigurationStorage
    {
        Task SaveChanges(
            Models.Client.ConfigurationSettings configurationSettings);
    }
}
