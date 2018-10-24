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
        private readonly IConfigurationStorage _configurationStorage;
        private readonly IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IEventDispatcher _viewEventDispatcher;

        private CancellationTokenSource _cancellation;
        private PlayerDeckTrackerView _view;
        private PlayerDeckTrackerViewModel _viewModel;

        public WindowedPlayerDeckTrackerInterface(
            ICardInfoProvider cardInfoProvider,
            IConfigurationSource configurationSource,
            IConfigurationStorage configurationStorage,
            IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            IEventDispatcher viewEventDispatcher)
        {
            _cardInfoProvider =
                cardInfoProvider.Require(nameof(cardInfoProvider));

            _configurationSource =
                configurationSource.Require(nameof(configurationSource));

            _configurationStorage =
                configurationStorage.Require(nameof(configurationStorage));

            _eventDispatcherFactory =
                eventDispatcherFactory.Require(nameof(eventDispatcherFactory));

            _eventStreamFactory =
                eventStreamFactory.Require(nameof(eventStreamFactory));

            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));
        }

        void IPlayerDeckTrackerInterface.Close()
        {
            Reset();
        }

        private void Reset()
        {
            _cancellation?.Cancel();

            _viewModel?.Cleanup();

            _view?.Close();
        }

        void IPlayerDeckTrackerInterface.TrackDeck(
            Decklist decklist)
        {
            Reset();

            IEventDispatcher eventDispatcher = _eventDispatcherFactory.Create();

            _viewModel =
                new PlayerDeckTrackerViewModel(
                    _cardInfoProvider,
                    _configurationSource,
                    _configurationStorage,
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
                            object @event = await eventStream.ReadNext();

                            if (_cancellation.IsCancellationRequested)
                                return;

                            eventDispatcher.DispatchEvent(@event);
                        }
                    }
                });

            _view = new PlayerDeckTrackerView(_viewModel);

            _view.Show();
        }
    }
}
