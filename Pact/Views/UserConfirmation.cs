using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class UserConfirmation
        : IUserConfirmation
    {
        private readonly IModalDisplay _modalDisplay;

        public UserConfirmation(
            IModalDisplay modalDisplay)
        {
            _modalDisplay = modalDisplay.Require(nameof(modalDisplay));
        }

        Task<bool> IUserConfirmation.Confirm(
            string message,
            string accept,
            string decline)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class UserConfirmationViewModel
        : IModalViewModel<bool>
    {
        // text fields

        public event Action<bool> OnClosed;
    }
}
