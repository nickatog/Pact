using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DownloadUpdatesViewModel
    {
        #region Private members
        private readonly ICardDatabaseUpdateInterface _cardDatabaseUpdateInterface;
        #endregion // Private members

        public DownloadUpdatesViewModel(
            #region Dependency assignments
            ICardDatabaseUpdateInterface cardDatabaseUpdateInterface)
        {
            _cardDatabaseUpdateInterface =
                cardDatabaseUpdateInterface.Require(nameof(cardDatabaseUpdateInterface));
            #endregion // Dependency assignments
        }

        public ICommand CheckForUpdates =>
            new DelegateCommand(
                () => _cardDatabaseUpdateInterface.CheckForUpdates());
    }
}
