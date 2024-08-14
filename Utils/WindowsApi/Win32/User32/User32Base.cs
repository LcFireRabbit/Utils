using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class User32Base
    {
        private const string User32 = "user32.dll";

        #region Field
        public const int GWL_HWNDPARENT = -8;

        public const int SE_PRIVILEGE_ENABLED = 0x00000002;

        public const int TOKEN_QUERY = 0x00000008;

        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        /// <summary>
        /// 检索桌面壁纸的位图文件的完整路径
        /// </summary>
        public const uint SPI_GETDESKWALLPAPER = 0x0073;
        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        // 映射 DEVMODE 结构
        // 可以参照 DEVMODE结构的指针定义：
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd183565(v=vs.85).aspx
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            public short dmLogPixels;
            public short dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct DevBroadcastDeviceInterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DevBroadcastHdr
        {
            internal UInt32 Size;
            internal UInt32 DeviceType;
            internal UInt32 Reserved;
        }

        [Flags]
        public enum ExitWindows : uint
        {
            /// <summary>
            /// 注销
            /// </summary>
            LoginOff = 0x00,
            /// <summary>
            /// 关机
            /// </summary>
            ShutDown = 0x01,
            /// <summary>
            /// 重启
            /// </summary>
            Reboot = 0x02,
            Force = 0x04,
            PowerOff = 0x08,
            ForceIfHung = 0x10
        }

        [Flags]
        public enum ShutdownReason : uint
        {
            MajorApplication = 0x00040000,
            MajorHardware = 0x00010000,
            MajorLegacyApi = 0x00070000,
            MajorOperatingSystem = 0x00020000,
            MajorOther = 0x00000000,
            MajorPower = 0x00060000,
            MajorSoftware = 0x00030000,
            MajorSystem = 0x00050000,
            MinorBlueScreen = 0x0000000F,
            MinorCordUnplugged = 0x0000000b,
            MinorDisk = 0x00000007,
            MinorEnvironment = 0x0000000c,
            MinorHardwareDriver = 0x0000000d,
            MinorHotfix = 0x00000011,
            MinorHung = 0x00000005,
            MinorInstallation = 0x00000002,
            MinorMaintenance = 0x00000001,
            MinorMMC = 0x00000019,
            MinorNetworkConnectivity = 0x00000014,
            MinorNetworkCard = 0x00000009,
            MinorOther = 0x00000000,
            MinorOtherDriver = 0x0000000e,
            MinorPowerSupply = 0x0000000a,
            MinorProcessor = 0x00000008,
            MinorReconfig = 0x00000004,
            MinorSecurity = 0x00000013,
            MinorSecurityFix = 0x00000012,
            MinorSecurityFixUninstall = 0x00000018,
            MinorServicePack = 0x00000010,
            MinorServicePackUninstall = 0x00000016,
            MinorTermSrv = 0x00000020,
            MinorUnstable = 0x00000006,
            MinorUpgrade = 0x00000003,
            MinorWMI = 0x00000015,
            FlagUserDefined = 0x40000000,
            FlagPlanned = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        #endregion

        #region P/Invoke Functions

        [DllImport(User32)]
        internal static extern int Mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport(User32)]
        internal static extern int Keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport(User32)]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport(User32)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        /// <summary>
        /// 设置窗体父窗体
        /// </summary>
        /// <param name="hWndChild">子窗体句柄</param>
        /// <param name="hWndNewParent">父窗体句柄</param>
        /// <returns></returns>
        [DllImport(User32)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// 得到对应名称Window的句柄
        /// </summary>
        /// <param name="lpWindowClass">Window所属类</param>
        /// <param name="lpWindowName">Window名称</param>
        /// <returns></returns>
        [DllImport(User32, SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);

        /// <summary>
        /// 得到指定窗口的子窗口
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        internal delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);
        [DllImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport(User32, SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport(User32, SetLastError = true)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport(User32, SetLastError = true)]
        internal static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport(User32)]
        internal static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 枚举显示器设置
        /// </summary>
        /// <param name="deviceName">显示器名称</param>
        /// <param name="modeNum">参数类型</param>
        /// <param name="devMode">参数结构</param>
        /// <returns></returns>
        [DllImport(User32)]
        internal static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        /// <summary>
        /// 改变显示设置
        /// </summary>
        /// <param name="devMode">参数结构</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport(User32)]
        internal static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        /// <summary>
        /// 注册设备通知
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="notificationFilter"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        /// <summary>
        /// 取消设备通知
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(User32)]
        internal static extern bool UnregisterDeviceNotification(IntPtr handle);

        [DllImport(User32)]
        internal static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        /// <summary>
        /// 检索或设置系统范围参数(sting类型)
        /// </summary>
        /// <param name="uAction"></param>
        /// <param name="uParam"></param>
        /// <param name="lpvParam"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SystemParametersInfo(uint uAction, uint uParam, StringBuilder lpvParam, uint init);
        #endregion
    }
}
