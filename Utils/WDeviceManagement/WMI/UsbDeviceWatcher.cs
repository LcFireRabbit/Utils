using System;
using System.Collections.Specialized;
using System.IO;
using System.Management;

namespace Utils.WDeviceManagement.WMI
{
    public class UsbDeviceWatcher
    {
        /// <summary>
        /// Keyboard
        /// </summary>
        private static readonly Guid Keyboard = new Guid("{4d36e96b-e325-11ce-bfc1-08002be10318}");

        /// <summary>
        /// Mouse
        /// </summary>
        private static readonly Guid Mouse = new Guid("{4d36e96f-e325-11ce-bfc1-08002be10318}");

        /// <summary>
        /// USB设备上线
        /// </summary>
        public event EventHandler<UsbStorageCreatEventArgs> UsbStorageInserted;
        /// <summary>
        /// USB设备下线
        /// </summary>
        public event EventHandler<UsbStorageDeleteEventArgs> UsbStorageRemoved;

        /// <summary>
        /// HID鼠标上线
        /// </summary>
        public event EventHandler<PnPEntityInfo> HIDMouseInserted;
        /// <summary>
        /// HID鼠标下线无需求,可以通过验证PnpDeviceId来确定设备
        /// </summary>
        //public event EventHandler<PnPEntityInfo> HIDMouseRemoved;

        /// <summary>
        /// HID键盘上线
        /// </summary>
        public event EventHandler<PnPEntityInfo> HIDKeyboardInserted;
        /// <summary>
        /// HID键盘下线,无需求,可以通过验证PnpDeviceId来确定设备
        /// </summary>
        //public event EventHandler<PnPEntityInfo> HIDKeyboardRemoved;


        private ManagementEventWatcher _insertWatcher;

        private ManagementEventWatcher _removeWatcher;

        public class UsbStorageCreatEventArgs : EventArgs
        {
            public StringCollection DiskRoot { get; set; }

            public DriveType DriveType { get; set; }

            public string PNPDeviceID { get; set; }

            /// <summary>
            /// 通过得到的设备描述，来查询需要的信息
            /// </summary>
            /// <param name="text"></param>
            /// <param name="dependent"></param>
            public UsbStorageCreatEventArgs(string text, string dependent)
            {
                DiskRoot = UsbDeviceInfo.WMI_GetDiskRoot(dependent);

                DriveType = UsbDeviceInfo.WMI_GetDiskType(dependent);

                PNPDeviceID = text;
            }
        }

        public class UsbStorageDeleteEventArgs : EventArgs
        {
            /// <summary>
            /// 设备的唯一标识
            /// </summary>
            public string PNPDeviceID { get; set; }

            public UsbStorageDeleteEventArgs(string text)
            {
                PNPDeviceID = text;
            }
        }

        /// <summary>
        /// 获取UsbDeviceWatcher的实例
        /// </summary>
        public static UsbDeviceWatcher Instance = new Lazy<UsbDeviceWatcher>(() => new UsbDeviceWatcher()).Value;

        /// <summary>
        /// 单例模式
        /// </summary>
        private UsbDeviceWatcher() { }

        /// <summary>
        /// 添加USB设备监视
        /// </summary>
        /// <returns></returns>
        public bool AddUSBEventWatcher()
        {
            try
            {
                var scope = new ManagementScope("root\\CIMV2");
                var insert = new WqlEventQuery("__InstanceCreationEvent", TimeSpan.FromSeconds(1), "TargetInstance isa 'Win32_USBControllerDevice'");
                var remove = new WqlEventQuery("__InstanceDeletionEvent", TimeSpan.FromSeconds(1), "TargetInstance isa 'Win32_USBControllerDevice'");

                _insertWatcher = new ManagementEventWatcher(scope, insert);
                _removeWatcher = new ManagementEventWatcher(scope, remove);

                ///WMI服务USB加载响应事件
                _insertWatcher.EventArrived += OnUSBInserted;
                ///WMI服务USB移除响应事件
                _removeWatcher.EventArrived += OnUSBRemoved;

                ///开启监听
                _insertWatcher.Start();
                _removeWatcher.Start();

                return true;
            }
            catch (Exception)
            {
                Dispose();
                return false;
            }
        }

        /// <summary>
        /// 注销Usb设备监视
        /// </summary>
        public void Dispose()
        {
            if (_insertWatcher != null)
            {
                _insertWatcher.Stop();
                _insertWatcher.Dispose();
            }
            if (_removeWatcher != null)
            {
                _removeWatcher.Stop();
                _removeWatcher.Dispose();
            }
        }

        /// <summary>
        /// Usb设备下线处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUSBRemoved(object sender, EventArrivedEventArgs e)
        {
            string dependent = UsbDeviceInfo.WhoUsbControllerDevice(e).Dependent;
            string text = dependent.Replace("\\\\", "\\");

            ///Usb存储类设备标志
            if (text.StartsWith("USBSTOR\\"))
            {
                UsbStorageRemoved?.Invoke(this, new UsbStorageDeleteEventArgs(text));
            }
        }

        /// <summary>
        /// Usb设备上线处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUSBInserted(object sender, EventArrivedEventArgs e)
        {
            string dependent = UsbDeviceInfo.WhoUsbControllerDevice(e).Dependent;
            string text = dependent.Replace("\\\\", "\\");

            ///Usb存储类设备标志
            if (text.StartsWith("USBSTOR\\"))
            {
                UsbStorageInserted?.Invoke(this, new UsbStorageCreatEventArgs(text, dependent));
            }
            else if (text.StartsWith("HID\\"))
            {
                PnPEntityInfo[] pnPEntityInfos = UsbDeviceInfo.WhoPnPEntity(text);

                for (int i = 0; !(pnPEntityInfos == null) && i < pnPEntityInfos.Length; i++)
                {
                    ///通过guid去判定当前上线设备是什么类别的设备
                    if (pnPEntityInfos[i].ClassGuid == Mouse)
                    {
                        HIDMouseInserted?.Invoke(this, pnPEntityInfos[i]);
                    }
                    else if (pnPEntityInfos[i].ClassGuid == Keyboard)
                    {
                        HIDKeyboardInserted?.Invoke(this, pnPEntityInfos[i]);
                    }
                }
            }
        }
    }
}
