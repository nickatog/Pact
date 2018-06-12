#region Namespaces
using System;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class PlayerDeckTrackerViewModelFactory
        : IPlayerDeckTrackerViewModelFactory
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IConfigurationStorage _configurationStorage;
        private readonly ITrackedCardViewModelFactory _trackedCardViewModelFactory;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public PlayerDeckTrackerViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage,
            ITrackedCardViewModelFactory trackedCardViewModelFactory,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

            _configurationStorage =
                configurationStorage
                ?? throw new ArgumentNullException(nameof(configurationStorage));

            _trackedCardViewModelFactory =
                trackedCardViewModelFactory
                ?? throw new ArgumentNullException(nameof(trackedCardViewModelFactory));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));
        }
        #endregion // Constructors

        PlayerDeckTrackerViewModel IPlayerDeckTrackerViewModelFactory.Create(
            IEventDispatcher gameEventDispatcher,
            Decklist decklist)
        {
            return
                new PlayerDeckTrackerViewModel(
                    _cardInfoProvider,
                    _configurationSource,
                    _configurationStorage,
                    gameEventDispatcher,
                    _trackedCardViewModelFactory,
                    _viewEventDispatcher,
                    decklist);
        }
    }
}
