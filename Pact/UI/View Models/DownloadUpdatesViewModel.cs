using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class DownloadUpdatesViewModel
    {
        #region Private members
        private readonly ICardInfoDatabaseUpdateInterface _cardInfoDatabaseUpdateInterface;
        #endregion // Private members

        public DownloadUpdatesViewModel(
            #region Dependency assignments
            ICardInfoDatabaseUpdateInterface cardInfoDatabaseUpdateInterface)
        {
            _cardInfoDatabaseUpdateInterface =
                cardInfoDatabaseUpdateInterface.Require(nameof(cardInfoDatabaseUpdateInterface));
            #endregion // Dependency assignments
        }

        public ICommand CheckForUpdates =>
            new DelegateCommand(
                () => _cardInfoDatabaseUpdateInterface.CheckForUpdates());
    }
}
