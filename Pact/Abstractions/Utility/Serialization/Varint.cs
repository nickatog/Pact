using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Pact.Extensions.Contract;

namespace Pact
{
    public static class Varint
    {
        public static Task<int> Parse(
            Stream stream)
        {
            stream.Require(nameof(stream));

            return __Parse();

            async Task<int> __Parse()
            {
                int result = 0;

                int bytesRead = 0;

                var byteValue = new byte[1];
                while (await stream.ReadAsync(byteValue, 0, 1).ConfigureAwait(false) != 0)
                {
                    int shiftedValue = (byteValue[0] & 0x7F) << bytesRead * 7;

                    result += shiftedValue;

                    bytesRead++;

                    if ((byteValue[0] & 0x80) == 0)
                        break;
                }

                return result;
            }
        }

        public static IEnumerable<byte> GetBytes(
            int value)
        {
            do
            {
                int byteValue = value & 0x7F;
                if (value > 0x7F)
                    byteValue |= 0x80;

                yield return (byte)byteValue;

                value = value >> 7;
            } while (value > 0);
        }
    }
}
