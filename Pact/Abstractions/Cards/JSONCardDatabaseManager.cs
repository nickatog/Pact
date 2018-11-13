using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Valkyrie;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class JSONCardDatabaseManager
        : ICardDatabaseManager
    {
        #region Private members
        private static readonly string s_appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private const string CARD_DATABASE_FILENAME = "cards.json";
        private const string CARD_DATABASE_VERSION_FILENAME = "cards.version";

        private readonly IEventDispatcher _viewEventDispatcher;
        #endregion // Private members

        public JSONCardDatabaseManager(
            #region Dependency assignments
            IEventDispatcher viewEventDispatcher)
        {
            _viewEventDispatcher =
                viewEventDispatcher.Require(nameof(viewEventDispatcher));
            #endregion // Dependency assignments
        }

        int? ICardDatabaseManager.GetCurrentVersion()
        {
            string text = null;

            try
            {
                text = File.ReadAllText(Path.Combine(s_appDirectory, CARD_DATABASE_VERSION_FILENAME));
            }
            catch (FileNotFoundException)
            {
            }

            if (int.TryParse(text, out int version))
                return version;

            return null;
        }

        async Task ICardDatabaseManager.UpdateCardDatabase(
            int version,
            Stream updateStream)
        {
            updateStream.Require(nameof(updateStream));

            string tempFilePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Pact",
                    $"~{CARD_DATABASE_FILENAME}");

            using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create))
                await updateStream.CopyToAsync(tempFileStream);

            string targetFilePath = Path.Combine(s_appDirectory, CARD_DATABASE_FILENAME);

            File.Copy(tempFilePath, targetFilePath, true);

            File.Delete(tempFilePath);

            File.WriteAllText(Path.Combine(s_appDirectory, CARD_DATABASE_VERSION_FILENAME), version.ToString());

            _viewEventDispatcher.DispatchEvent(new Events.CardDatabaseUpdated());
        }
    }
}
