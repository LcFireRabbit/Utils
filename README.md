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
    ```
    - USB上线设备信息查询(WMI/Cfgmgr32)
    ```C#
        /// <summary>
        /// 定位发生插拔的USB设备
        /// </summary>
        /// <param name="e">USB插拔事件参数</param>
        /// <returns>发生插拔现象的USB控制设备ID</returns>
        public static UsbControllerDevice WhoUsbControllerDevice(EventArrivedEventArgs e)
        {
            ManagementBaseObject managementBaseObject = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (managementBaseObject != null && managementBaseObject.ClassPath.ClassName == "Win32_USBControllerDevice")
            {
                string text = (managementBaseObject["Antecedent"] as string).Split('=')[1];
                string antecedent = text.Substring(1, text.Length - 2);
                string text2 = (managementBaseObject["Dependent"] as string).Split('=')[1];
                string dependent = text2.Substring(1, text2.Length - 2);
                return new UsbControllerDevice
                {
                    Antecedent = antecedent,
                    Dependent = dependent
                };
            }
            return null;
        }
    /// <summary>
    /// 通过得到的设备描述，来查询需要的信息
    /// </summary>
    /// <param name="text"></param>
    /// <param name="dependent"></param>
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
    ```
 - 设备管理(SetupDi)
    - 查询设备信息
    - 禁用设备
    - 卸载设备
    - 禁用U盘(注册表)
