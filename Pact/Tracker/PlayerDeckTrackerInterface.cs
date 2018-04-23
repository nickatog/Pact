using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class PlayerDeckTrackerInterface
        : IDeckTrackerInterface
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSettings _configurationSettings;

        private PlayerDeckTrackerView _view;
        private PlayerDeckTrackerViewModel _viewModel;

        public PlayerDeckTrackerInterface(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSettings configurationSettings)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSettings = configurationSettings.Require(nameof(configurationSettings));
        }

        void IDeckTrackerInterface.Close()
        {
            _viewModel?.Cleanup();

            _view?.Hide();
        }

        void IDeckTrackerInterface.StartTracking(
            Valkyrie.IEventDispatcher gameEventDispatcher,
            Valkyrie.IEventDispatcher viewEventDispatcher,
            Decklist decklist)
        {
            _viewModel =
                new PlayerDeckTrackerViewModel(
                    _cardInfoProvider,
                    _configurationSettings,
                    gameEventDispatcher,
                    viewEventDispatcher,
                    decklist);

            _view = PlayerDeckTrackerView.GetWindowFor(_viewModel);

            _view.Show();
        }
    }
}
