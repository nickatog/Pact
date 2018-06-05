#region Namespaces
using Pact.Extensions.Contract;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModelFactory
        : IPlayerDeckTrackerViewModelFactory
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public PlayerDeckTrackerViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }
        #endregion // Constructors

        PlayerDeckTrackerViewModel IPlayerDeckTrackerViewModelFactory.Create(
            IEventDispatcher gameEventDispatcher,
            Decklist decklist)
        {
            return
                new PlayerDeckTrackerViewModel(
                    _cardInfoProvider,
                    _configurationSettings,
                    gameEventDispatcher,
                    _viewEventDispatcher,
                    decklist);
        }
    }
}
