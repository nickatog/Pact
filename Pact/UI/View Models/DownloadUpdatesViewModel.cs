using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DownloadUpdatesViewModel
    {
        private readonly ICardDatabaseUpdateInterface _cardDatabaseUpdateInterface;

        public DownloadUpdatesViewModel(
            ICardDatabaseUpdateInterface cardDatabaseUpdateInterface)
        {
            _cardDatabaseUpdateInterface = cardDatabaseUpdateInterface.Require(nameof(cardDatabaseUpdateInterface));
        }

        public ICommand CheckForUpdates =>
            new DelegateCommand(
                () => _cardDatabaseUpdateInterface.CheckForUpdates());
    }
}
