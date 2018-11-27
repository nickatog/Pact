using System.Threading;
using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class WindowedPlayerDeckTrackerInterface
        : IPlayerDeckTrackerInterface
    {
        private readonly ICardInfoProvider _cardInfoProvider;
        private readonly IConfigurationSource _configurationSource;
        private readonly IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IEventDispatcher _viewEventDispatcher;

        private CancellationTokenSource _cancellation;
        private PlayerDeckTrackerView _view;
        private PlayerDeckTrackerViewModel _viewModel;

        public WindowedPlayerDeckTrackerInterface(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider = cardInfoProvider.Require(nameof(cardInfoProvider));
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }

        void IPlayerDeckTrackerInterface.Close()
        {
            Reset();
        }

        void IPlayerDeckTrackerInterface.TrackDeck(
            Models.Client.Decklist decklist)
        {
            Reset();

            IEventDispatcher eventDispatcher = _eventDispatcherFactory.Create();

            _viewModel =
                new PlayerDeckTrackerViewModel(
                    _cardInfoProvider,
                    _configurationSource,
                    eventDispatcher,
                    _viewEventDispatcher,
                    decklist);
            
            _cancellation = new CancellationTokenSource();
            
            Task.Run(
                async () =>
                {
                    using (IEventStream eventStream = _eventStreamFactory.Create())
                    {
                        while (true)
                        {
                            object @event = await eventStream.ReadNext(_cancellation.Token);
                            if (@event == null)
                                return;

                            eventDispatcher.DispatchEvent(@event);
                        }
                    }
                });

            _view = new PlayerDeckTrackerView(_viewModel);

            _view.Show();
        }

        private void Reset()
        {
            _cancellation?.Cancel();
            _viewModel?.Cleanup();
            _view?.Close();
        }
    }
}
