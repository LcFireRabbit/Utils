using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static byte SetBit(byte data, int bitIndex, bool value)
        {
            if (bitIndex < 0 || bitIndex >= 8)
            {
                throw new ArgumentException("Invalid parameters.");
            }

            if (value)
            {
                // Set the bit at bitPosition in data[byteIndex]
                data |= (byte)(1 << bitIndex);
            }
            else
            {
                // Clear the bit at bitPosition in data[byteIndex]
                data &= (byte)~(1 << bitIndex);
            }

            return data;
        }

        public static bool GetBit(byte[] data, int bitIndex)
        {
            if (data == null || bitIndex < 0 || bitIndex >= data.Length * 8)
            {
                throw new ArgumentException("Invalid parameters.");
            }

            int byteIndex = bitIndex / 8;
            int bitPosition = bitIndex % 8;

            return GetBit(data[byteIndex], bitPosition);
        }

        public static bool GetBit(byte data, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 8)
            {
                throw new ArgumentException("Invalid parameters.");
            }

            bool bit = (data & (1 << bitIndex)) != 0;
            return bit;
        }
    }
}
