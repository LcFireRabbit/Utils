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
    - USB上线设备信息查询
 - 设备管理(SetupDi)
    - 查询设备信息
    - 禁用设备
    - 卸载设备
    - 禁用U盘(注册表)
