using Microsoft.Win32;

namespace Utils.WDeviceManagement.Regedit
{
    public class UsbStorageService
    {
        #region 禁用启用USB存储
        /// <summary>
        /// 禁用USB存储
        /// </summary>
        public static void DisableUSBStorage()
        {
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", 4, RegistryValueKind.DWord);
        }

        /// <summary>
        /// 启用USB存储
        /// </summary>
        public static void EnableUSBStorage()
        {
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", 3, RegistryValueKind.DWord);
        }
        #endregion
    }
}
