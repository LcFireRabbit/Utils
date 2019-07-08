using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WDeviceManagement.WMI
{
    public class UsbDeviceInfo
    {
        public class UsbControllerDevice
        {
            public string Antecedent;

            public string Dependent;
        }

        public enum CM_LOCATE_DEVNODE_FLAG : uint
        {
            CM_LOCATE_DEVNODE_NORMAL = 0u,
            CM_LOCATE_DEVNODE_PHANTOM = 1u,
            CM_LOCATE_DEVNODE_CANCELREMOVE = 2u,
            CM_LOCATE_DEVNODE_NOVALIDATION = 4u,
            CM_LOCATE_DEVNODE_BITS = 7u
        }


        [DllImport("Cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern uint CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, int ulFlags = 0);
        [DllImport("Cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern uint CM_Get_Device_ID(uint dnDevInst, [Out] StringBuilder Buffer, int BufferLen, int ulFlags = 0);
        [DllImport("Cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern uint CM_Locate_DevNode(out uint pdnDevInst, string pDeviceID, CM_LOCATE_DEVNODE_FLAG ulFlags = CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_NORMAL);
        [DllImport("Cfgmgr32.dll", CharSet = CharSet.Unicode)]
        private static extern uint CM_Get_DevNode_Registry_Property(uint dnDevInst, uint ulProperty, IntPtr pulRegDataType, ref int Buffer, ref int pulLength, uint ulFlags = 0u);


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
        /// 得到当前可移动存储设备的盘符
        /// </summary>
        /// <param name="PNPDeviceID"></param>
        /// <returns></returns>
        public static StringCollection WMI_GetLogicalDrives(string PNPDeviceID)
        {
            StringCollection stringCollection = new StringCollection();
            var list = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE PNPDeviceID='" + PNPDeviceID + "'").Get();
            foreach (ManagementObject item in new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE PNPDeviceID='" + PNPDeviceID + "'").Get())
            {
                foreach (ManagementObject item2 in item.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject item3 in item2.GetRelated("Win32_LogicalDisk"))
                    {
                        stringCollection.Add(item3["Name"] as string);
                    }
                }
            }
            return stringCollection;
        }

        /// <summary>
        /// 通过字节点的DEVID找到父节点的DEVID
        /// </summary>
        /// <param name="ChildDevID"></param>
        /// <param name="ParentDevNode"></param>
        /// <param name="ParentDevID"></param>
        /// <param name="ulFlags"></param>
        /// <returns></returns>
        public static uint CM_GetParentDevIDByChildDevID(string ChildDevID, out uint ParentDevNode, out string ParentDevID,
            CM_LOCATE_DEVNODE_FLAG ulFlags = CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_NORMAL)
        {
            ParentDevNode = uint.MaxValue;
            ParentDevID = null;
            uint pdnDevInst;
            uint num = CM_Locate_DevNode(out pdnDevInst, ChildDevID, ulFlags);
            if (num == 0)
            {
                num = CM_Get_Parent(out ParentDevNode, pdnDevInst);
                if (num == 0)
                {
                    int num2 = 201;
                    StringBuilder stringBuilder = new StringBuilder(num2);
                    num = CM_Get_Device_ID(ParentDevNode, stringBuilder, num2);
                    if (num == 0)
                    {
                        ParentDevID = stringBuilder.ToString();
                    }
                }
            }
            return num;
        }

        /// <summary>
        /// 通过字节点的DevNode找到父节点的DEVID
        /// </summary>
        /// <param name="ChildDevNode"></param>
        /// <param name="ParentDevNode"></param>
        /// <param name="ParentDevID"></param>
        /// <param name="ulFlags"></param>
        /// <returns></returns>
        public static uint CM_GetParentDevIDByChildDevNode(uint ChildDevNode, out uint ParentDevNode, out string ParentDevID,
            CM_LOCATE_DEVNODE_FLAG ulFlags = CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_NORMAL)
        {
            ParentDevID = null;
            uint num = CM_Get_Parent(out ParentDevNode, ChildDevNode);
            if (num == 0)
            {
                int num2 = 201;
                StringBuilder stringBuilder = new StringBuilder(num2);
                num = CM_Get_Device_ID(ParentDevNode, stringBuilder, num2);
                if (num == 0)
                {
                    ParentDevID = stringBuilder.ToString();
                }
            }
            return num;
        }

        /// <summary>
        /// 得到DevNode的地址
        /// </summary>
        /// <param name="DevNode"></param>
        /// <returns></returns>
        public static int CM_GetDevNodeAddress(uint DevNode)
        {
            int Buffer = 0;
            int pulLength = Marshal.SizeOf(typeof(int));
            if (CM_Get_DevNode_Registry_Property(DevNode, 29u, IntPtr.Zero, ref Buffer, ref pulLength) == 0)
            {
                return Buffer;
            }
            return -1;
        }
    }
}
