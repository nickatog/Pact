using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;
using Pact.Extensions.Enumerable;

namespace Pact
{
    public sealed class FileBasedPowerLogManager
        : IPowerLogManager
    {
        private readonly AsyncSemaphore _asyncMutex;
        private readonly IConfigurationSource _configurationSource;
        private readonly string _directoryPath;
        private readonly string _manifestFileName;
        private readonly ICollectionSerializer<Models.Data.SavedLog> _savedLogCollectionSerializer;
        private readonly IEventDispatcher _viewEventDispatcher;

        private readonly IList<IEventHandler> _eventHandlers = new List<IEventHandler>();
        private string _powerLogFilePath;

        public FileBasedPowerLogManager(
            AsyncSemaphore asyncMutex,
            IConfigurationSource configurationSource,
            string directoryPath,
            string manifestFileName,
            ICollectionSerializer<Models.Data.SavedLog> savedLogCollectionSerializer,
            IEventDispatcher viewEventDispatcher)
        {
            _asyncMutex = asyncMutex.Require(nameof(asyncMutex));
            _configurationSource = configurationSource.Require(nameof(configurationSource));
            _directoryPath = directoryPath.Require(nameof(directoryPath));
            _manifestFileName = manifestFileName.Require(nameof(manifestFileName));
            _savedLogCollectionSerializer = savedLogCollectionSerializer.Require(nameof(savedLogCollectionSerializer));
            _viewEventDispatcher = viewEventDispatcher.Require(nameof(viewEventDispatcher));

            _powerLogFilePath = _configurationSource.GetSettings().PowerLogFilePath;

            _eventHandlers.Add(
                new DelegateEventHandler<ViewEvents.ConfigurationSettingsSaved>(
                    __event => _powerLogFilePath = _configurationSource.GetSettings().PowerLogFilePath));

            _eventHandlers.ForEach(__eventHandler => _viewEventDispatcher.RegisterHandler(__eventHandler));
        }

        async Task IPowerLogManager.DeleteSavedLog(
            Guid savedLogID)
        {
            try
            {
                File.Delete(Path.Combine(_directoryPath, $"{savedLogID}.txt"));
            }
            catch (FileNotFoundException) {}

            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
                await PersistSavedLogManifest(
                    (await ParseSavedLogManifest().ConfigureAwait(false))
                    .Where(__savedLog => __savedLog.ID != savedLogID)).ConfigureAwait(false);
        }

        async Task<IEnumerable<Models.Client.SavedLog>> IPowerLogManager.GetSavedLogs()
        {
            return
                (await ParseSavedLogManifest().ConfigureAwait(false))
                .Select(
                    __savedLog =>
                        new Models.Client.SavedLog(
                            __savedLog.ID,
                            Path.Combine(_directoryPath, $"{__savedLog.ID}.txt"),
                            __savedLog.Title,
                            __savedLog.Timestamp));
        }

        async Task<Models.Client.SavedLog?> IPowerLogManager.SaveCurrentLog(
            string title)
        {
            if (!File.Exists(_powerLogFilePath))
                return null;

            var directoryInfo = new DirectoryInfo(_directoryPath);
            if (!directoryInfo.Exists)
                directoryInfo.Create();

            var savedLogID = Guid.NewGuid();
            string savedLogFilePath = Path.Combine(_directoryPath, $"{savedLogID}.txt");
            var timestamp = DateTimeOffset.Now;

            using (var targetStream = new FileStream(savedLogFilePath, FileMode.Create))
                using (var sourceStream = new FileStream(_powerLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
                await PersistSavedLogManifest(
                    (await ParseSavedLogManifest().ConfigureAwait(false))
                    .Append(new Models.Data.SavedLog(savedLogID, title, timestamp))).ConfigureAwait(false);

            return new Models.Client.SavedLog(savedLogID, savedLogFilePath, title, timestamp);
        }

        async Task IPowerLogManager.UpdateSavedLog(
            Models.Client.SavedLogDetail savedLogDetail)
        {
            using (await _asyncMutex.WaitAsync().ConfigureAwait(false))
                await PersistSavedLogManifest(
                    (await ParseSavedLogManifest().ConfigureAwait(false))
                    .Select(
                        __savedLog =>
                        {
                            if (__savedLog.ID != savedLogDetail.ID)
                                return __savedLog;

                            return
                                new Models.Data.SavedLog(
                                    __savedLog.ID,
                                    savedLogDetail.Title,
                                    __savedLog.Timestamp);
                        })).ConfigureAwait(false);
        }

        private async Task<IEnumerable<Models.Data.SavedLog>> ParseSavedLogManifest()
        {
            string manifestFilePath = Path.Combine(_directoryPath, _manifestFileName);
            if (!File.Exists(manifestFilePath))
                return Enumerable.Empty<Models.Data.SavedLog>();

            using (var stream = new FileStream(manifestFilePath, FileMode.Open))
                return await _savedLogCollectionSerializer.Deserialize(stream).ConfigureAwait(false);
        }

        private async Task PersistSavedLogManifest(
            IEnumerable<Models.Data.SavedLog> savedLogs)
        {
            using (var stream = new FileStream(Path.Combine(_directoryPath, _manifestFileName), FileMode.Create))
                await _savedLogCollectionSerializer.Serialize(stream, savedLogs).ConfigureAwait(false);
        }
    }
}
