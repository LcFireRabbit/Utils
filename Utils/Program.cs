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

            _usbDeviceWatcher.USBInserted += UsbInsertedHandler;

            _usbDeviceWatcher.USBRemoved += UsbRemovedHandler;
        }

        /// <summary>
        /// 下线处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UsbRemovedHandler(object sender, UsbDeviceWatcher.UsbStorageDeleteEventArgs e)
        {

        }

        /// <summary>
        /// 上线处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UsbInsertedHandler(object sender, UsbDeviceWatcher.UsbStorageCreatEventArgs e)
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
