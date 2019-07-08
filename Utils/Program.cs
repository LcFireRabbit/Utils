using System;
using System.Threading.Tasks;
using Utils.WDeviceManagement.WMI;

namespace Utils
{
    class Program
    {
        static UsbDeviceWatcher _usbDeviceWatcher;

        static void Main(string[] args)
        {
            UsbMonitor();

            Console.ReadLine();

            Dispose();
        }

        static void UsbMonitor()
        {
            _usbDeviceWatcher = UsbDeviceWatcher.Instance;

            _usbDeviceWatcher.AddUSBEventWatcher();

            _usbDeviceWatcher.USBInserted += UsbInsertedHandler;

            _usbDeviceWatcher.USBRemoved += UsbRemovedHandler;
        }

        private static void UsbRemovedHandler(object sender, UsbDeviceWatcher.UsbStorageDeleteEventArgs e)
        {

        }

        private static void UsbInsertedHandler(object sender, UsbDeviceWatcher.UsbStorageCreatEventArgs e)
        {

        }

        static void Dispose()
        {
            _usbDeviceWatcher.Dispose();
        }
    }
}
