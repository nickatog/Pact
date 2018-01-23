using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pact.Extensions.Contract;

namespace Pact
{
    public sealed class Base64VarintDecklistSerializer
        : IDecklistSerializer
    {
        private readonly ICardInfoProvider _cardInfoProvider;

        public Base64VarintDecklistSerializer(
            ICardInfoProvider cardInfoProvider)
        {
            _cardInfoProvider = cardInfoProvider.OrThrow(nameof(cardInfoProvider));
        }

        async Task<Decklist> IDecklistSerializer.Deserialize(
            Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            string deckstring = null;
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Length <= 0 || line[0] == '#')
                        continue;

                    deckstring = line;

                    break;
                }
            }

            if (deckstring == null)
                throw new Exception("Failed to find deckstring!");

            using (var stringStream = new MemoryStream(Convert.FromBase64String(deckstring)))
            {
                stringStream.Seek(3, SeekOrigin.Begin);

                int count = ParseVarint(stringStream);
                var heroes = new int[count];
                for (int counter = 0; counter < count; counter++)
                    heroes[counter] = ParseVarint(stringStream);

                count = ParseVarint(stringStream);
                var singleCards = new int[count];
                for (int counter = 0; counter < count; counter++)
                    singleCards[counter] = ParseVarint(stringStream);

                count = ParseVarint(stringStream);
                var doubleCards = new int[count];
                for (int counter = 0; counter < count; counter++)
                    doubleCards[counter] = ParseVarint(stringStream);

                count = ParseVarint(stringStream);
                var variableCards = new(int, int)[count];
                for (int counter = 0; counter < count; counter++)
                    variableCards[counter] = (ParseVarint(stringStream), ParseVarint(stringStream));

                return
                    new Decklist(
                        _cardInfoProvider.GetCardInfo(heroes.FirstOrDefault())?.ID,
                        singleCards.Select(__databaseID => (_cardInfoProvider.GetCardInfo(__databaseID)?.ID, 1))
                        .Concat(doubleCards.Select(__databaseID => (_cardInfoProvider.GetCardInfo(__databaseID)?.ID, 2)))
                        .ToList());
            }
        }

        Task IDecklistSerializer.Serialize(
            Stream stream,
            Decklist decklist)
        {
            throw new NotImplementedException();
        }

        private static int ParseVarint(Stream stream)
        {
            int result = 0;

            int bytesRead = 0;

            int byteValue;
            while ((byteValue = stream.ReadByte()) != -1)
            {
                int shiftedValue = (byteValue & 0x7F) << bytesRead * 7;

                result += shiftedValue;

                bytesRead++;

                if ((byteValue & 0x80) == 0)
                    break;
            }

            return result;
        }
    }
}
