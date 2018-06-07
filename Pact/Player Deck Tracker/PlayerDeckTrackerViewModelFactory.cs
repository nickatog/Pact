#region Namespaces
using System;
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
        private readonly ITrackedCardViewModelFactory _trackedCardViewModelFactory;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public PlayerDeckTrackerViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            ITrackedCardViewModelFactory trackedCardViewModelFactory,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));

            _trackedCardViewModelFactory =
                trackedCardViewModelFactory
                ?? throw new ArgumentNullException(nameof(trackedCardViewModelFactory));

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
                    _trackedCardViewModelFactory,
                    _viewEventDispatcher,
                    decklist);
        }
    }
}
