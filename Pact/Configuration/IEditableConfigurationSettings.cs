using System;
using System.Threading.Tasks;

namespace Pact
{
    public interface IEditableConfigurationSettings
        : IConfigurationSettings
    {
        new int CardTextOffset { get; set; }

        Task SaveChanges(
            IEditableConfigurationSettings configurationSettings,
            Action<IEditableConfigurationSettings> configurationChanges);
    }
}
