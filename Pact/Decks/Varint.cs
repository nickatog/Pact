using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pact
{
    public static class Varint
    {
        public static async Task<int> Parse(
            Stream stream)
        {
            int result = 0;

            int bytesRead = 0;

            var byteValue = new byte[1];
            while (await stream.ReadAsync(byteValue, 0, 1) != 0)
            {
                int shiftedValue = (byteValue[0] & 0x7F) << bytesRead * 7;

                result += shiftedValue;

                bytesRead++;

                if ((byteValue[0] & 0x80) == 0)
                    break;
            }

            return result;
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
