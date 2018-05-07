using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class ModalUserConfirmation
        : IUserConfirmation
    {
        private readonly IModalDisplay _modalDisplay;
        private readonly IUserConfirmationModalViewModelFactory _viewModelFactory;

        public ModalUserConfirmation(
            IModalDisplay modalDisplay,
            IUserConfirmationModalViewModelFactory viewModelFactory)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
            _viewModelFactory = viewModelFactory.Require(nameof(viewModelFactory));
        }

        Task<bool> IUserConfirmation.Confirm(
            string message,
            string accept,
            string decline)
        {
            var result = new TaskCompletionSource<bool>();

            _modalDisplay.Show(
                _viewModelFactory.Create(message, accept, decline),
                __result => result.SetResult(__result));

            return result.Task;
        }
    }
}
