using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class WinmmExtension:WinmmBase
    {
        private const uint iMaxValue = 0xFFFF;
        private const uint iMinValue = 0x0000;
        private static int iCurrentValue = 0;

        public static void SetSystemVolume(double arg)
        {
            double newVolume = ushort.MaxValue * arg / 10.0;

            uint v = ((uint)newVolume) & 0xffff;
            uint vAll = v | (v << 16);

            int retVal = WaveOutSetVolume(IntPtr.Zero, vAll);
        }

        private static void GetVolume()
        {
            //uint d, v;
            //d = 0;
            //long i = WaveOutGetVolume(d, out v);
            //uint vleft = v & 0xFFFF;
            //uint vright = (v & 0xFFFF0000) >> 16;
            //uint all = vleft | vright;
            //uint value = (all * uint.Parse((MaxValue - MinValue).ToString()) / ((UInt32)iMaxValue));
            //iCurrentValue = int.Parse(value.ToString());
        }
    }
}
