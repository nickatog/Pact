using Pact.Extensions.Contract;

namespace Pact
{
    public interface IPlayerDeckTrackerViewModelFactory
    {
        PlayerDeckTrackerViewModel Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Decklist decklist);
    }

    public sealed class PlayerDeckTrackerViewModelFactory
        : IPlayerDeckTrackerViewModelFactory
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly Valkyrie.IEventDispatcher _viewEventDispatcher;

        public PlayerDeckTrackerViewModelFactory(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings,
            Valkyrie.IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }

        PlayerDeckTrackerViewModel IPlayerDeckTrackerViewModelFactory.Create(
            Valkyrie.IEventDispatcher gameEventDispatcher,
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
