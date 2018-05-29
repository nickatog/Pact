using System.Threading;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class PlayerDeckTrackerInterface
        : IDeckTrackerInterface
    {
        private readonly Valkyrie.IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IPlayerDeckTrackerViewModelFactory _playerDeckTrackerViewModelFactory;

        private CancellationTokenSource _cancellation;
        private PlayerDeckTrackerView _view;
        private PlayerDeckTrackerViewModel _viewModel;

        public PlayerDeckTrackerInterface(
            Valkyrie.IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            IPlayerDeckTrackerViewModelFactory playerDeckTrackerViewModelFactory)
        {
            _eventDispatcherFactory = eventDispatcherFactory.Require(nameof(eventDispatcherFactory));
            _eventStreamFactory = eventStreamFactory.Require(nameof(eventStreamFactory));
            _playerDeckTrackerViewModelFactory = playerDeckTrackerViewModelFactory.Require(nameof(playerDeckTrackerViewModelFactory));
        }

        void IDeckTrackerInterface.Close()
        {
            Reset();

            _view?.Hide();
        }

        private void Reset()
        {
            _cancellation?.Cancel();

            _viewModel?.Cleanup();
        }

        void IDeckTrackerInterface.StartTracking(
            Decklist decklist)
        {
            Reset();

            Valkyrie.IEventDispatcher trackerEventDispatcher = _eventDispatcherFactory.Create();

            _viewModel = _playerDeckTrackerViewModelFactory.Create(trackerEventDispatcher, decklist);
            
            _cancellation = new CancellationTokenSource();

            Task.Run(
                async () =>
                {
                    IEventStream eventStream = _eventStreamFactory.Create();

                    while (true)
                    {
                        object @event = await eventStream.ReadNext();

                        if (_cancellation.IsCancellationRequested)
                            return;

                        trackerEventDispatcher.DispatchEvent(@event);
                    }
                });

            _view = PlayerDeckTrackerView.GetWindowFor(_viewModel);

            _view.Show();
        }
    }
}
