using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Drive
    {
        /// <summary>
        /// 分区是否在外部磁盘上
        /// </summary>
        /// <param name="driveLetter">分区盘符</param>
        /// <returns></returns>
        public static bool IsDriveOnExternalDisk(string driveLetter)
        {
            bool retVal = false;
            driveLetter = driveLetter.TrimEnd('\\');

            // browse all USB WMI physical disks
            foreach (ManagementObject drive in new ManagementObjectSearcher("select DeviceID, MediaType,InterfaceType from Win32_DiskDrive").Get())
            {
                // associate physical disks with partitions
                ManagementObjectCollection partitionCollection = new ManagementObjectSearcher(String.Format("associators of {{Win32_DiskDrive.DeviceID='{0}'}} " + "where AssocClass = Win32_DiskDriveToDiskPartition", drive["DeviceID"])).Get();

                foreach (ManagementObject partition in partitionCollection)
                {
                    if (partition != null)
                    {
                        // associate partitions with logical disks (drive letter volumes)
                        ManagementObjectCollection logicalCollection = new ManagementObjectSearcher(String.Format("associators of {{Win32_DiskPartition.DeviceID='{0}'}} " + "where AssocClass= Win32_LogicalDiskToPartition", partition["DeviceID"])).Get();

                        foreach (ManagementObject logical in logicalCollection)
                        {
                            if (logical != null)
                            {
                                // finally find the logical disk entry
                                ManagementObjectCollection.ManagementObjectEnumerator volumeEnumerator = new ManagementObjectSearcher(String.Format("select DeviceID from Win32_LogicalDisk " + "where Name='{0}'", logical["Name"])).Get().GetEnumerator();

                                volumeEnumerator.MoveNext();

                                ManagementObject volume = (ManagementObject)volumeEnumerator.Current;

                                if (driveLetter.ToLowerInvariant().Equals(volume["DeviceID"].ToString().ToLowerInvariant()) &&
                                    (drive["MediaType"].ToString().ToLowerInvariant().Contains("external") || drive["InterfaceType"].ToString().ToLowerInvariant().Contains("usb")))
                                {
                                    retVal = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return retVal;
        }
    }
}
