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
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Dependencies

        #region Constructors
        public TrackedCardViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider
                ?? throw new ArgumentNullException(nameof(cardInfoProvider));

            _configurationSource =
                configurationSource
                ?? throw new ArgumentNullException(nameof(configurationSource));

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
                    _configurationSource,
                    gameEventDispatcher,
                    _viewEventDispatcher,
                    cardID,
                    count,
                    playerID);
        }
    }
}
