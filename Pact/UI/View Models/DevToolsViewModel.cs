using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DevToolsViewModel
    {
        private readonly ILogManagementInterface _logManagementInterface;

        public DevToolsViewModel(
            ILogManagementInterface logManagementInterface)
        {
            _logManagementInterface = logManagementInterface.Require(nameof(logManagementInterface));
        }

        public ICommand ManageSavedLogs =>
            new DelegateCommand(
                () => _logManagementInterface.ManageLogs());
    }
}
