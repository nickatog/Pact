using System;
using System.Diagnostics;
using System.Windows.Input;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class SavedLogViewModel
    {
        private readonly IPowerLogManager _powerLogManager;
        private readonly IEventDispatcher _viewEventDispatcher;
        
        private readonly string _filePath;
        private readonly string _textEditorFilePath;

        public SavedLogViewModel(
            IConfigurationSource configurationSource,
            IPowerLogManager powerLogManager,
            IEventDispatcher viewEventDispatcher,
            Guid id,
            string title,
            DateTimeOffset timestamp,
            string filePath)
        {
            configurationSource.Require(nameof(configurationSource));

            _powerLogManager = powerLogManager.Require(nameof(powerLogManager));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            ID = id;
            Title = title ?? string.Empty;
            Timestamp = timestamp;
            _filePath = filePath;

            _textEditorFilePath = configurationSource.GetSettings().TextEditorFilePath;
        }

        public ICommand Delete =>
            new DelegateCommand(
                () => _viewEventDispatcher.DispatchEvent(new ViewCommands.DeleteSavedLog(ID)));

        public Guid ID { get; }

        public ICommand SaveTitle =>
            new DelegateCommand(
                async () => await _powerLogManager.UpdateSavedLog(new Models.Client.SavedLogDetail(ID, Title)));

        public DateTimeOffset Timestamp { get; }

        public string Title { get; set; }

        public ICommand ViewLogFile =>
            new DelegateCommand(
                () =>
                {
                    try
                    {
                        Process.Start(_textEditorFilePath, _filePath);
                    }
                    catch {}
                });
    }
}
