#region Namespaces
using System;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class TrackedCardViewModelFactory
        : ITrackedCardViewModelFactory
    {
        #region Dependencies
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public TrackedCardViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _configurationSettings =
                configurationSettings
                ?? throw new ArgumentNullException(nameof(configurationSettings));

            _viewEventDispatcher =
                viewEventDispatcher
                ?? throw new ArgumentNullException(nameof(viewEventDispatcher));
        }
        #endregion // Constructors

        TrackedCardViewModel ITrackedCardViewModelFactory.Create(
            IEventDispatcher gameEventDispatcher,
            string cardID,
            int count,
            int? playerID)
        {
            return
                new TrackedCardViewModel(
                    _cardInfoProvider,
                    _configurationSettings,
                    gameEventDispatcher,
                    _viewEventDispatcher,
                    cardID,
                    count,
                    playerID);
        }
    }
}
