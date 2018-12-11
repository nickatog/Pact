using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Valkyrie;

using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class LogManagementModalViewModel
        : IModalViewModel<object>
        , INotifyPropertyChanged
    {
        private readonly IConfigurationSource _configurationSource;
        private readonly IPowerLogManager _powerLogManager;
        private readonly IEventDispatcher _viewEventDispatcher;

        private IList<SavedLogViewModel> _logViewModels;
        private IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();

        public LogManagementModalViewModel(
            IConfigurationSource configurationSource,
            IPowerLogManager powerLogManager,
            IEventDispatcher viewEventDispatcher)
        {
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _powerLogManager = powerLogManager.Require(nameof(powerLogManager));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _powerLogManager.GetSavedLogs()
            .ContinueWith(
                __task =>
                {
                    _logViewModels =
                        new ObservableCollection<SavedLogViewModel>(
                            __task.Result
                            .OrderByDescending(__savedLog => __savedLog.Timestamp)
                            .Select(__savedLog => CreateSavedLogViewModel(__savedLog)));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogViewModels)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogCount)));
                });

            _viewEventHandlers.Add(
                new DelegateEventHandler<ViewCommands.DeleteSavedLog>(
                    async __event =>
                    {
                        try
                        {
                            await _powerLogManager.DeleteSavedLog(__event.SavedLogID);
                        }
                        catch (Exception)
                        {
                            // error message?

                            return;
                        }

                        SavedLogViewModel viewModel =
                            _logViewModels
                            .FirstOrDefault(__viewModel => __viewModel.ID == __event.SavedLogID);
                        if (viewModel == null)
                            return;

                        _logViewModels.Remove(viewModel);
                    }));

            _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.RegisterHandler(__handler));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<object> OnClosed;

        public ICommand Close =>
            new DelegateCommand(
                () =>
                {
                    _viewEventHandlers.ForEach(__handler => _viewEventDispatcher.UnregisterHandler(__handler));

                    OnClosed?.Invoke(null);
                });

        public string LogCount => (_logViewModels?.Count() ?? 0).ToString();

        public string LogTitle { get; set; }

        public IEnumerable<SavedLogViewModel> LogViewModels => _logViewModels;
        
        public ICommand SaveCurrentLog =>
            new DelegateCommand(
                async () =>
                {
                    // lock UI?

                    Models.Client.SavedLog? newLog = await _powerLogManager.SaveCurrentLog(LogTitle);
                    if (!newLog.HasValue)
                        return;

                    _logViewModels.Insert(0, CreateSavedLogViewModel(newLog.Value));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogCount)));
                });

        private SavedLogViewModel CreateSavedLogViewModel(
            Models.Client.SavedLog savedLog)
        {
            return
                new SavedLogViewModel(
                    _configurationSource,
                    _powerLogManager,
                    _viewEventDispatcher,
                    savedLog.ID,
                    savedLog.Title,
                    savedLog.Timestamp,
                    savedLog.FilePath);
        }
    }
}
