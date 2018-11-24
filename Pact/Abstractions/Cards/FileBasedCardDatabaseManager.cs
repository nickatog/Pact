using System;
using System.IO;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class FileBasedCardDatabaseManager
        : ICardDatabaseManager
    {
        private readonly string _cardDatabaseFilePath;
        private readonly string _cardDatabaseVersionFilePath;

        public FileBasedCardDatabaseManager(
            string cardDatabaseFilePath,
            string cardDatabaseVersionFilePath)
        {
            _cardDatabaseFilePath = cardDatabaseFilePath.Require(nameof(cardDatabaseFilePath));
            _cardDatabaseVersionFilePath = cardDatabaseVersionFilePath.Require(nameof(cardDatabaseVersionFilePath));
        }

        int? ICardDatabaseManager.GetCurrentVersion()
        {
            string text = null;

            try
            {
                text = File.ReadAllText(_cardDatabaseVersionFilePath);
            }
            catch (FileNotFoundException) {}

            if (int.TryParse(text, out int version))
                return version;

            return null;
        }

        Task ICardDatabaseManager.UpdateCardDatabase(
            int version,
            Stream updateStream)
        {
            updateStream.Require(nameof(updateStream));

            return __UpdateCardDatabase();

            async Task __UpdateCardDatabase()
            {
                string tempFilePath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        $@"Pact\~{Path.GetFileName(_cardDatabaseFilePath)}");

                var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(tempFilePath));
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                using (var tempFileStream = new FileStream(tempFilePath, FileMode.Create))
                    await updateStream.CopyToAsync(tempFileStream).ConfigureAwait(false);

                File.Copy(tempFilePath, _cardDatabaseFilePath, true);

                File.Delete(tempFilePath);

                File.WriteAllText(_cardDatabaseVersionFilePath, version.ToString());
            }
        }
    }
}
