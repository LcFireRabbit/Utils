﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Management;
using System.Text;

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
            public string DiskRoot { get; set; }

            public string InstanceId { get; set; }

            /// <summary>
            /// 通过得到的设备描述，来查询需要的信息
            /// </summary>
            /// <param name="text"></param>
            /// <param name="dependent"></param>
            public UsbStorageCreatEventArgs(string text, string dependent)
            {
                UsbDeviceInfo.CM_Locate_DevNode(out uint pdnDevInst, text);

                StringCollection stringCollection =UsbDeviceInfo.WMI_GetDiskRoot(dependent);

                int num2 = 512;
                StringBuilder stringBuilder = new StringBuilder(num2);
                var result = UsbDeviceInfo.CM_Get_Device_ID(pdnDevInst, stringBuilder, num2);
                if (result == 0)
                    Console.WriteLine(stringBuilder.ToString());
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

            ///用来判断U盘上线
            if (text.StartsWith("USBSTOR\\"))
            {
                DriveType driverType = UsbDeviceInfo.WMI_GetDiskType(dependent);

                if (ReferenceEquals(driverType, DriveType.Removable))
                {
                    USBInserted?.Invoke(this, new UsbStorageCreatEventArgs(text, dependent));

                }
                else
                {
                    ///卸载或者禁用设备
                }
            }
            ///其他usb设备
            else
            {
                ///得到他的guid 判断是鼠标类，还是键盘类
                ///查询user的配置，看当前设备是否支持使用键盘或者鼠标
                ///支持，直接无作为；不支持，禁用所有该类型设备。
            }
        }
    }
}
