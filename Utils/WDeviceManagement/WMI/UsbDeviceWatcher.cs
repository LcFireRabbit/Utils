using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WDeviceManagement.WMI
{
    public class UsbDeviceWatcher
    {
        /// <summary>
        /// USB设备上线
        /// </summary>
        public event EventHandler<UsbStorageCreatEventArgs> USBInserted;
        /// <summary>
        /// USB设备下线
        /// </summary>
        public event EventHandler<UsbStorageDeleteEventArgs> USBRemoved;

        private ManagementEventWatcher _insertWatcher;

        private ManagementEventWatcher _removeWatcher;

        public class UsbStorageCreatEventArgs : EventArgs
        {
            public uint UsbDevNode { get; set; }

            public string UsbDevID { get; set; }

            public string UsbHubDevID { get; set; }

            public string ServerID { get; set; }

            public int ConnectionIndex { get; set; }

            public string UsbPath { get; set; }

            public UsbStorageCreatEventArgs(string text, string dependent)
            {
                if (UsbDeviceInfo.CM_GetParentDevIDByChildDevID(text, out uint ParentDevNode, out string ParentDevID) != 0
                    || UsbDeviceInfo.CM_GetParentDevIDByChildDevNode(ParentDevNode, out uint _, out string ParentDevID2) != 0)
                {
                    return;
                }
                int num = UsbDeviceInfo.CM_GetDevNodeAddress(ParentDevNode);
                if (num != -1)
                {
                    if (text.StartsWith("USBSTOR\\"))
                    {
                        StringCollection logicalDiskCollection = UsbDeviceInfo.WMI_GetLogicalDrives(dependent);
                        UsbPath = logicalDiskCollection[0] + "\\";
                    }
                    
                    UsbDevNode = ParentDevNode;
                    UsbDevID = ParentDevID;
                    UsbHubDevID = ParentDevID2;
                    ConnectionIndex = num;
                    ServerID = text;
                }
            }
        }

        public class UsbStorageDeleteEventArgs : EventArgs
        {
            public string ServerID { get; set; }
            public UsbStorageDeleteEventArgs(string text)
            {
                ServerID = text;
            }
        }

        /// <summary>
        /// 获取UsbDeviceWatcher的实例
        /// </summary>
        public static UsbDeviceWatcher Instance = new Lazy<UsbDeviceWatcher>(() => new UsbDeviceWatcher()).Value;

        /// <summary>
        /// 单例模式
        /// </summary>
        private UsbDeviceWatcher(){}

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

                _insertWatcher.EventArrived += OnUSBInserted;
                _removeWatcher.EventArrived += OnUSBRemoved;

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
            USBRemoved?.Invoke(this, new UsbStorageDeleteEventArgs(text));
            ///用来判断U盘下线
            //if (text.StartsWith("USBSTOR\\"))
            //{
                
            //}
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
            USBInserted?.Invoke(this, new UsbStorageCreatEventArgs(text, dependent));

            ///用来判断U盘上线
            //if (text.StartsWith("USBSTOR\\"))
            //{
            //}
        }
    }
}
