using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class LogManagementModalViewModel
        : IModalViewModel<object>
        , INotifyPropertyChanged
    {
        private readonly IPowerLogManager _powerLogManager;

        private IList<SavedLogViewModel> _logViewModels;

        public LogManagementModalViewModel(
            IPowerLogManager powerLogManager)
        {
            _powerLogManager = powerLogManager.Require(nameof(powerLogManager));

            _powerLogManager.GetSavedLogs()
            .ContinueWith(
                __task =>
                    _logViewModels =
                        new ObservableCollection<SavedLogViewModel>(
                            __task.Result
                            .Select(__savedLog => CreateSavedLogViewModel(__savedLog))))
            .ContinueWith(
                __ =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogViewModels)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogCount)));
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<object> OnClosed;

        public ICommand Close =>
            new DelegateCommand(
                () => OnClosed?.Invoke(null));

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

                    _logViewModels.Add(CreateSavedLogViewModel(newLog.Value));


                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogCount)));
                });

        private SavedLogViewModel CreateSavedLogViewModel(
            SavedLog savedLog)
        {
            return
                new SavedLogViewModel(
                    savedLog.ID,
                    savedLog.Title,
                    savedLog.Timestamp,
                    savedLog.FilePath);
        }
    }

    public sealed class SavedLogViewModel
    {
        private readonly string _editorPath;
        private readonly Guid _id;
        private readonly string _filePath;

        public SavedLogViewModel(
            Guid id,
            string title,
            DateTimeOffset timestamp,
            string filePath)
        {
            _id = id;
            Title = title;
            Timestamp = timestamp;
            _filePath = filePath;
        }

        public DateTimeOffset Timestamp { get; }

        public string Title { get; }

        public ICommand ViewLogFile =>
            new DelegateCommand(
                () =>
                {
                    Process.Start("notepad", _filePath);
                });
    }
}
