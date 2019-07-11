using System;
using System.Threading.Tasks;
using Utils.WDeviceManagement.SetupDi;
using Utils.WDeviceManagement.WMI;
namespace Utils
{
    class Program
    {
        static UsbDeviceWatcher _usbDeviceWatcher;

        static void Main(string[] args)
        {
            UsbMonitor();

            //SetupDiExtension.ChangeMouseState(SetupDiExtension.State.DISABLE);

            Console.ReadLine();

            //SetupDiExtension.ScanForHardWareChanges();

            //SetupDiExtension.ChangeMouseState(SetupDiExtension.State.ENABLE);

            Dispose();
        }

        /// <summary>
        /// Usb设备上下线监控方法
        /// </summary>
        static void UsbMonitor()
        {
            _usbDeviceWatcher = UsbDeviceWatcher.Instance;

            _usbDeviceWatcher.AddUSBEventWatcher();

            _usbDeviceWatcher.UsbStorageInserted += UsbStorageInsertedHandler;

            _usbDeviceWatcher.UsbStorageRemoved += UsbStorageRemovedHandler;

            _usbDeviceWatcher.HIDMouseInserted += HIDMouseInsertedHandler;

            _usbDeviceWatcher.HIDKeyboardInserted += HIDKeyboardInsertedHadnler;
        }

        /// <summary>
        /// HID键盘上线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HIDKeyboardInsertedHadnler(object sender, PnPEntityInfo e)
        {

        }

        /// <summary>
        /// HID鼠标上线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HIDMouseInsertedHandler(object sender, PnPEntityInfo e)
        {

        }

        /// <summary>
        /// Usb存储设备下线处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UsbStorageRemovedHandler(object sender, UsbDeviceWatcher.UsbStorageDeleteEventArgs e)
        {

        }

        /// <summary>
        /// Usb存储设备上线处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UsbStorageInsertedHandler(object sender, UsbDeviceWatcher.UsbStorageCreatEventArgs e)
        {

        }

        /// <summary>
        /// 销毁
        /// </summary>
        static void Dispose()
        {
            _usbDeviceWatcher.Dispose();
        }
    }
}
