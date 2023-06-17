using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class User32Extension : User32Base
    {
        #region Field
        private const int WM_SYSCOMMAND = 0x112; //系统消息
        private const int SC_SCREENSAVE = 0xf140; // 启动屏幕保护消息
        private const int SC_MONITORPOWER = 0xF170; //关闭显示器的系统命令
        private const int POWER_OFF = 2; //2 为关闭, 1为省电状态，-1为开机
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff); //广播消息，所有顶级窗体都会接收
        //移动鼠标 
        public const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        //模拟鼠标滚轮滚动操作，必须配合dwData参数
        public const int MOUSEEVENTF_WHEEL = 0x0800;

        // 控制改变屏幕分辨率的常量
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;

        // 控制改变方向的常量定义
        public const int DMDO_DEFAULT = 0;
        public const int DMDO_90 = 1;
        public const int DMDO_180 = 2;
        public const int DMDO_270 = 3;

        //注册设备消息相关
        public const int DbtDeviceArrival = 0x8000; // System detected a new device        
        public const int DbtDeviceRemoveComplete = 0x8004; // Device is gone      
        public const int WmDeviceChange = 0x0219; // Device change event    
        private const int DbtDeviceTypeDeviceInterface = 5;
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-usb-device
        private static readonly Guid GuidDeviceInterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices
        // https://docs.microsoft.com/en-us/windows-hardware/drivers/install/guid-devinterface-monitor
        private static readonly Guid GuidDeviceInterfaceMonitorDevice = new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7"); // Monitor devices
        private static IntPtr usbNotificationHandle;
        private static IntPtr monitorNotificationHandle;
        #endregion

        #region Function
        /// <summary>
        /// 虚拟鼠标操作
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dData"></param>
        /// <param name="dwExtraInfo"></param>
        public static void SimulateMouseOperation(int flags, int dx, int dy, int dData, int dwExtraInfo)
        {
            User32Base.Mouse_event(flags, dx, dy, dData, dwExtraInfo);
        }

        /// <summary>
        /// 进行一次简单的鼠标操作
        /// </summary>
        public static void ClearFreeTime()
        {
            User32Base.Keybd_event(39, 0, 0, 0);
            User32Base.Keybd_event(39, 0, 2, 0);
        }

        /// <summary>
        /// 获取电脑最后输入时间
        /// </summary>
        /// <returns></returns>
        public static int GetLastInputTime()
        {
            int idleTime = 0;

            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();

            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);

            lastInputInfo.dwTime = 0;

            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = Convert.ToInt32(lastInputInfo.dwTime);

                idleTime = envTicks - lastInputTick;
            }

            int toret = ((idleTime > 0) ? (idleTime / 1000) : 0);

            return toret;
        }

        /// <summary>
        /// 关闭显示器
        /// </summary>
        public static void TurnOffScreen()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, POWER_OFF); // 关闭显示器
        }

        /// <summary>
        /// 返回窗口子窗口指针列表
        /// Returns a list of child windows
        /// </summary>
        /// <param name="parent">Parent of the windows to return</param>
        /// <returns>List of child windows</returns>
        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        /// <summary>
        /// 枚举子窗口的回调方法
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        /// <summary>
        /// 改变分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static ExecuteResult ChangeDisplayResolution(int width, int height)
        {
            // 初始化 DEVMODE结构
            DEVMODE devmode = new DEVMODE();
            devmode.dmDeviceName = new String(new char[32]);
            devmode.dmFormName = new String(new char[32]);
            devmode.dmSize = (short)Marshal.SizeOf(devmode);

            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devmode))
            {
                devmode.dmPelsWidth = width;
                devmode.dmPelsHeight = height;

                // 改变屏幕分辨率
                int iRet = ChangeDisplaySettings(ref devmode, CDS_TEST);

                if (iRet == DISP_CHANGE_FAILED)
                {
                    //MessageBox.Show("不能执行你的请求", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return ExecuteResult.Failed;
                }
                else
                {
                    iRet = ChangeDisplaySettings(ref devmode, CDS_UPDATEREGISTRY);

                    switch (iRet)
                    {
                        // 成功改变
                        case DISP_CHANGE_SUCCESSFUL:
                            {
                                return ExecuteResult.Success;
                                //break;
                            }
                        case DISP_CHANGE_RESTART:
                            {
                                // MessageBox.Show("你需要重新启动电脑设置才能生效", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return ExecuteResult.ReStart;
                                //break;
                            }
                        default:
                            {
                                return ExecuteResult.Failed;
                                // MessageBox.Show("改变屏幕分辨率失败", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                //break;
                            }
                    }
                }
            }
            else
            {
                return ExecuteResult.Failed;
            }
        }

        /// <summary>
        /// 获取显示设置
        /// </summary>
        /// <returns></returns>
        public static DEVMODE GetDisplaySettings()
        {
            // 初始化 DEVMODE结构
            DEVMODE devmode = new DEVMODE();
            devmode.dmDeviceName = new String(new char[32]);
            devmode.dmFormName = new String(new char[32]);
            devmode.dmSize = (short)Marshal.SizeOf(devmode);

            User32Base.EnumDisplaySettings(null, User32Extension.ENUM_CURRENT_SETTINGS, ref devmode);

            return devmode;
        }

        #region 注册设备消息相关
        public static bool IsMonitor(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceMonitorDevice);
        }

        public static bool IsUsbDevice(IntPtr lParam)
        {
            return IsDeviceOfClass(lParam, GuidDeviceInterfaceUSBDevice);
        }
        /// Registers a window to receive notifications when Monitor devices are plugged or unplugged.
        public static void RegisterMonitorDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceMonitorDevice);
            monitorNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        public static void RegisterUsbDeviceNotification(IntPtr windowHandle)
        {
            var dbi = CreateBroadcastDeviceInterface(GuidDeviceInterfaceUSBDevice);
            usbNotificationHandle = RegisterDeviceNotification(dbi, windowHandle);
        }

        /// UnRegisters the window for Monitor device notifications
        public static void UnRegisterMonitorDeviceNotification()
        {
            UnregisterDeviceNotification(monitorNotificationHandle);
        }

        /// UnRegisters the window for USB device notifications
        public static void UnRegisterUsbDeviceNotification()
        {
            UnregisterDeviceNotification(usbNotificationHandle);
        }

        private static bool IsDeviceOfClass(IntPtr lParam, Guid classGuid)
        {
            var hdr = Marshal.PtrToStructure<DevBroadcastHdr>(lParam);

            if (hdr.DeviceType != DbtDeviceTypeDeviceInterface)
                return false;

            var devIF = Marshal.PtrToStructure<DevBroadcastDeviceInterface>(lParam);

            return devIF.ClassGuid == classGuid;

        }

        private static IntPtr RegisterDeviceNotification(DevBroadcastDeviceInterface dbi, IntPtr windowHandle)
        {
            var buffer = Marshal.AllocHGlobal(dbi.Size);
            IntPtr handle;

            try
            {
                Marshal.StructureToPtr(dbi, buffer, true);

                handle = RegisterDeviceNotification(windowHandle, buffer, 0);
            }
            finally
            {
                // Free buffer
                Marshal.FreeHGlobal(buffer);
            }

            return handle;
        }

        private static DevBroadcastDeviceInterface CreateBroadcastDeviceInterface(Guid classGuid)
        {
            var dbi = new DevBroadcastDeviceInterface
            {
                DeviceType = DbtDeviceTypeDeviceInterface,
                Reserved = 0,
                ClassGuid = classGuid,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);

            return dbi;
        }
        #endregion

        /// <summary>
        /// 重启
        /// </summary>
        public static void ReBoot()
        {
            DoExitWindows(ExitWindows.Force | ExitWindows.Reboot);
        }

        /// <summary>
        /// 关机
        /// </summary>
        public static void ShutDown()
        {
            DoExitWindows(ExitWindows.Force | ExitWindows.ShutDown);
        }

        /// <summary>
        /// 注销
        /// </summary>
        public static void LoginOff()
        {
            DoExitWindows(ExitWindows.Force | ExitWindows.LoginOff);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="flag"></param>
        private static void DoExitWindows(ExitWindows flag)
        {
            TokPriv1Luid tp;
            IntPtr hproc = Kernel32Base.GetCurrentProcess();
            Advapi32Base.OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            Advapi32Base.LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            Advapi32Base.AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ExitWindowsEx(flag, ShutdownReason.MajorOther);
        }
        #endregion
    }
}
