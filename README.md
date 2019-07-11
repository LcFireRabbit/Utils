# Windows的基础功能的代码实现

> **Windows设备管理器**
 - USB设备
    - USB设备上线监测(WMI)
    ```C#
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
    ```
    - USB上线设备信息查询(WMI/Cfgmgr32)(具体代码见项目)
    ```C#
        /// 定位发生插拔的USB设备
        public static UsbControllerDevice WhoUsbControllerDevice(EventArrivedEventArgs e)
        
        /// 根据设备ID定位设备
        public static PnPEntityInfo[] WhoPnPEntity(String PNPDeviceID)
        
        /// 得到当前可移动存储设备的盘符
        public static StringCollection WMI_GetDiskRoot(string PNPDeviceID)
        
        /// 得到当前移动存储设备的类型
        public static DriveType WMI_GetDiskType(string PNPDeviceID)
    ```
 - 设备管理(SetupDi)
    - 查询设备信息
    - 禁用设备
    - 卸载设备
    - 禁用U盘(注册表)
