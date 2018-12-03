using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalLogManagementInterface
        : ILogManagementInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IPowerLogManager _powerLogManager;

        public ModalLogManagementInterface(
            IModalDisplay modalDisplay,
            IPowerLogManager powerLogManager)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _powerLogManager = powerLogManager.Require(nameof(powerLogManager));
        }

        Task ILogManagementInterface.ManageLogs()
        {
            var completionSource = new TaskCompletionSource<object>();

            _modalDisplay.Show(
                new LogManagementModalViewModel(_powerLogManager),
                __ => completionSource.SetResult(null));

            return completionSource.Task;
        }
    }
}
