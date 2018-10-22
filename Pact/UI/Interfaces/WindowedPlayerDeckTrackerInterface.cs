#region Namespaces
using System;
using System.Threading;
using System.Threading.Tasks;
using Valkyrie;
#endregion // Namespaces

namespace Pact
{
    public sealed class WindowedPlayerDeckTrackerInterface
        : IPlayerDeckTrackerInterface
    {
        #region Dependencies
        private readonly IEventDispatcherFactory _eventDispatcherFactory;
        private readonly IEventStreamFactory _eventStreamFactory;
        private readonly IPlayerDeckTrackerViewModelFactory _playerDeckTrackerViewModelFactory;
        #endregion // Dependencies

        #region Fields
        private CancellationTokenSource _cancellation;
        private PlayerDeckTrackerView _view;
        private PlayerDeckTrackerViewModel _viewModel;
        #endregion // Fields

        #region Constructors
        public WindowedPlayerDeckTrackerInterface(
            IEventDispatcherFactory eventDispatcherFactory,
            IEventStreamFactory eventStreamFactory,
            IPlayerDeckTrackerViewModelFactory playerDeckTrackerViewModelFactory)
        {
            _eventDispatcherFactory =
                eventDispatcherFactory
                ?? throw new ArgumentNullException(nameof(eventDispatcherFactory));

            _eventStreamFactory =
                eventStreamFactory
                ?? throw new ArgumentNullException(nameof(eventStreamFactory));

            _playerDeckTrackerViewModelFactory =
                playerDeckTrackerViewModelFactory
                ?? throw new ArgumentNullException(nameof(playerDeckTrackerViewModelFactory));
        }
        #endregion // Constructors

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

            _viewModel = _playerDeckTrackerViewModelFactory.Create(eventDispatcher, decklist);
            
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
