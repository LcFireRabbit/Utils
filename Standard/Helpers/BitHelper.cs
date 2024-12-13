using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standard.Helpers
{
    public static class BitHelper
    {
        public static void SetBit(byte[] data, int bitIndex, bool value)
        {
            if (data == null || bitIndex < 0 || bitIndex >= data.Length * 8)
            {
                throw new ArgumentException("Invalid parameters.");
            }

            int byteIndex = bitIndex / 8;
            int bitPosition = bitIndex % 8;

            if (value)
            {
                // Set the bit at bitPosition in data[byteIndex]
                data[byteIndex] |= (byte)(1 << bitPosition);
            }
            else
            {
                // Clear the bit at bitPosition in data[byteIndex]
                data[byteIndex] &= (byte)~(1 << bitPosition);
            }
        }
    }
}
