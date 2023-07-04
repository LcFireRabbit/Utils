using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Monitor
    {
        public static int GetDesktopMonitorNumber()
        {
            int number = 0;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DesktopMonitor");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    number++;
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine(e.Message);
            }

            return number;
        }
    }
}
