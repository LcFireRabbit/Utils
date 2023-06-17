using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class WinmmBase
    {
        private const string Winmm = "winmm.dll";

        [DllImport(Winmm, EntryPoint = "waveOutSetVolume")]
        internal static extern int WaveOutSetVolume(IntPtr deviceID, uint Volume);
        [DllImport(Winmm, EntryPoint = "waveOutGetVolume")]
        internal static extern int WaveOutGetVolume(IntPtr deviceID, out uint Volume);
    }
}
