using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalLogManagementInterface
        : ILogManagementInterface
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IPowerLogManager _powerLogManager;
        private readonly IEventDispatcher _viewEventDispatcher;

        public ModalLogManagementInterface(
            IModalDisplay modalDisplay,
            IPowerLogManager powerLogManager,
            IEventDispatcher viewEventDispatcher)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _powerLogManager = powerLogManager.Require(nameof(powerLogManager));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }

        Task ILogManagementInterface.ManageLogs()
        {
            var completionSource = new TaskCompletionSource<object>();

            _modalDisplay.Show(
                new LogManagementModalViewModel(_powerLogManager, _viewEventDispatcher),
                __ => completionSource.SetResult(null));

            return completionSource.Task;
        }
    }
}
