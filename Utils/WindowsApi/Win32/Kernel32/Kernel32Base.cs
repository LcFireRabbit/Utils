using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Kernel32Base
    {
        private const string Kernel32 = "kernel32.dll";

        #region Struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            internal int bias; // 以分钟为单位
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            internal string standardName; // 标准时间的名称
            internal SystemTime standardDate;
            [MarshalAs(UnmanagedType.I4)]
            internal int standardBias; // 标准偏移
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            internal string daylightName; // 夏令时的名称
            internal SystemTime daylightDate;
            [MarshalAs(UnmanagedType.I4)]
            internal int daylightBias; // 夏令时偏移
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DynamicTimeZoneInformation
        {
            [MarshalAs(UnmanagedType.I4)]
            internal int bias; // 偏移，以分钟为单位
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            internal string standardName; // 标准时间的名称
            internal SystemTime standardDate;
            [MarshalAs(UnmanagedType.I4)]
            internal int standardBias; // 标准偏移
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            internal string daylightName; // 夏令时的名称
            internal SystemTime daylightDate;
            [MarshalAs(UnmanagedType.I4)]
            internal int daylightBias; // 夏令时偏移
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x80)]
            internal string timeZoneKeyName; // 时区名
            [MarshalAs(UnmanagedType.Bool)]
            internal bool dynamicDaylightTimeDisabled; // 是否自动调整时钟的夏令时
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            [MarshalAs(UnmanagedType.U2)]
            internal ushort year; // 年
            [MarshalAs(UnmanagedType.U2)]
            internal ushort month; // 月
            [MarshalAs(UnmanagedType.U2)]
            internal ushort dayOfWeek; // 星期
            [MarshalAs(UnmanagedType.U2)]
            internal ushort day; // 日
            [MarshalAs(UnmanagedType.U2)]
            internal ushort hour; // 时
            [MarshalAs(UnmanagedType.U2)]
            internal ushort minute; // 分
            [MarshalAs(UnmanagedType.U2)]
            internal ushort second; // 秒
            [MarshalAs(UnmanagedType.U2)]
            internal ushort milliseconds; // 毫秒
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SecurityAttributes
        {
            public SecurityAttributes(object securityDescriptor)
            {
                this.lpSecurityDescriptor = securityDescriptor;
            }

            uint nLegnth = 12;
            object lpSecurityDescriptor;
            [MarshalAs(UnmanagedType.VariantBool)]
            bool bInheritHandle = true;
        }
        #endregion

        #region P/Invoke Functions
        //General
        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool CloseHandle(uint hHandle);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint GetLastError();

        //Semaphore
        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint CreateSemaphore(SecurityAttributes auth, int initialCount, int maximumCount, string name);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint WaitForSingleObject(uint hHandle, uint dwMilliseconds);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool ReleaseSemaphore(uint hHandle, int lReleaseCount, out int lpPreviousCount);

        //Memory Mapped Files
        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateFileMapping(uint hFile, SecurityAttributes lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport(Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        //PrivateProfile
        [DllImport(Kernel32, CharSet = CharSet.Unicode)]
        internal static extern int WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport(Kernel32, CharSet = CharSet.Unicode)]
        internal static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport(Kernel32, EntryPoint = "GetPrivateProfileString")]
        internal static extern uint GetPrivateProfileStringA(string section, string key, string def, Byte[] retVal, int size, string filePath);

        //读取和设置时区
        // 针对于旧Windows系统，如Windows XP
        [DllImport(Kernel32, CharSet = CharSet.Auto)]
        internal static extern int GetTimeZoneInformation(ref TimeZoneInformation lpTimeZoneInformation);

        [DllImport(Kernel32, CharSet = CharSet.Auto)]
        internal static extern bool SetTimeZoneInformation(ref TimeZoneInformation lpTimeZoneInformation);

        // 针对于新Windows系统，如Windows 7, Windows8, Windows10
        [DllImport(Kernel32, CharSet = CharSet.Auto)]
        internal static extern int GetDynamicTimeZoneInformation(ref DynamicTimeZoneInformation lpDynamicTimeZoneInformation);

        [DllImport(Kernel32, CharSet = CharSet.Auto)]
        internal static extern bool SetDynamicTimeZoneInformation(ref DynamicTimeZoneInformation lpDynamicTimeZoneInformation);

        // 提升进程权限的Windows API
        [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean CloseHandle(IntPtr hObject);

        // 系统时间相关
        [DllImport(Kernel32)]
        internal static extern void GetLocalTime(ref SystemTime lpSystemTime);

        [DllImport(Kernel32)]
        internal static extern bool SetLocalTime(ref SystemTime lpSystemTime);

        [DllImport(Kernel32)]
        internal static extern void GetSystemTime(ref SystemTime lpSystemTime);

        [DllImport(Kernel32)]
        internal static extern bool SetSystemTime(ref SystemTime lpSystemTime);
        #endregion
    }
}
