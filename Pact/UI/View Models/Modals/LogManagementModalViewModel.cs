using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IPowerLogManager _powerLogManager;
        private readonly IEventDispatcher _viewEventDispatcher;

        private IList<SavedLogViewModel> _logViewModels;
        private IList<IEventHandler> _viewEventHandlers = new List<IEventHandler>();

        public LogManagementModalViewModel(
            IPowerLogManager powerLogManager,
            IEventDispatcher viewEventDispatcher)
        {
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

                    SavedLog? newLog = await _powerLogManager.SaveCurrentLog(LogTitle);
                    if (!newLog.HasValue)
                        return;

                    _logViewModels.Insert(0, CreateSavedLogViewModel(newLog.Value));

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogCount)));
                });

        private SavedLogViewModel CreateSavedLogViewModel(
            SavedLog savedLog)
        {
            return
                new SavedLogViewModel(
                    _powerLogManager,
                    _viewEventDispatcher,
                    savedLog.ID,
                    savedLog.Title,
                    savedLog.Timestamp,
                    savedLog.FilePath);
        }
    }

    public sealed class SavedLogViewModel
    {
        private readonly string _editorPath = "notepad";
        private readonly string _filePath;
        private readonly IPowerLogManager _powerLogEventManager;
        private readonly IEventDispatcher _viewEventDispatcher;

        public SavedLogViewModel(
            IPowerLogManager powerLogEventManager,
            IEventDispatcher viewEventDispatcher,
            Guid id,
            string title,
            DateTimeOffset timestamp,
            string filePath)
        {
            _powerLogEventManager = powerLogEventManager.Require(nameof(powerLogEventManager));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            ID = id;
            Title = title ?? string.Empty;
            Timestamp = timestamp;
            _filePath = filePath;
        }

        public ICommand Delete =>
            new DelegateCommand(
                () => _viewEventDispatcher.DispatchEvent(new ViewCommands.DeleteSavedLog(ID)));

        public Guid ID { get; }

        public ICommand SaveTitle =>
            new DelegateCommand(
                async () => await _powerLogEventManager.UpdateSavedLog(new SavedLogDetail(ID, Title)));

        public DateTimeOffset Timestamp { get; }

        public string Title { get; set; }

        public ICommand ViewLogFile =>
            new DelegateCommand(
                () =>
                {
                    Process.Start(_editorPath, _filePath);
                });
    }
}
