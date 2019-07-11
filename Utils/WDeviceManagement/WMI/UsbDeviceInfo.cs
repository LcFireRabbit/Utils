using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.WDeviceManagement.WMI
{
    /// <summary>
    /// 即插即用设备信息结构
    /// </summary>
    public struct PnPEntityInfo
    {
        public String PNPDeviceID;      // 设备ID
        public String Name;             // 设备名称
        public String Description;      // 设备描述
        public String Service;          // 服务
        public String Status;           // 设备状态
        public UInt16 VendorID;         // 供应商标识
        public UInt16 ProductID;        // 产品编号 
        public Guid ClassGuid;          // 设备安装类GUID
    }
    /// <summary>
    /// 基于WMI获取USB设备信息
    /// </summary>
    public partial class UsbDeviceInfo
    {
        #region UsbDevice
        /// <summary>
        /// 获取所有的USB设备实体（过滤没有VID和PID的设备）
        /// </summary>
        public static PnPEntityInfo[] AllUsbDevices
        {
            get
            {
                return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, Guid.Empty);
            }
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(UInt16 VendorID, UInt16 ProductID, Guid ClassGuid)
        {
            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        UInt16 theVendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        if (VendorID != UInt16.MinValue && VendorID != theVendorID) continue;

                        UInt16 theProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        if (ProductID != UInt16.MinValue && ProductID != theProductID) continue;

                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                Guid theClassGuid = new Guid(Entity["ClassGuid"] as String);    // 设备安装类GUID
                                if (ClassGuid != Guid.Empty && ClassGuid != theClassGuid) continue;

                                PnPEntityInfo Element;
                                Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                Element.Name = Entity["Name"] as String;                // 设备名称
                                Element.Description = Entity["Description"] as String;  // 设备描述
                                Element.Service = Entity["Service"] as String;          // 服务
                                Element.Status = Entity["Status"] as String;            // 设备状态
                                Element.VendorID = theVendorID;     // 供应商标识
                                Element.ProductID = theProductID;   // 产品编号
                                Element.ClassGuid = theClassGuid;   // 设备安装类GUID

                                UsbDevices.Add(Element);
                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(UInt16 VendorID, UInt16 ProductID)
        {
            return WhoUsbDevice(VendorID, ProductID, Guid.Empty);
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(Guid ClassGuid)
        {
            return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, ClassGuid);
        }

        /// <summary>
        /// 查询USB设备实体（设备要求有VID和PID）
        /// </summary>
        /// <param name="PNPDeviceID">设备ID，可以是不完整信息</param>
        /// <returns>设备列表</returns>        
        public static PnPEntityInfo[] WhoUsbDevice(String PNPDeviceID)
        {
            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];
                    if (!String.IsNullOrEmpty(PNPDeviceID))
                    {   // 注意：忽视大小写
                        if (Dependent.IndexOf(PNPDeviceID, 1, PNPDeviceID.Length - 2, StringComparison.OrdinalIgnoreCase) == -1) continue;
                    }

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                PnPEntityInfo Element;
                                Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                Element.Name = Entity["Name"] as String;                // 设备名称
                                Element.Description = Entity["Description"] as String;  // 设备描述
                                Element.Service = Entity["Service"] as String;          // 服务
                                Element.Status = Entity["Status"] as String;            // 设备状态
                                Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识   
                                Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号                         // 产品编号
                                Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                                UsbDevices.Add(Element);
                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }

        /// <summary>
        /// 根据服务定位USB设备
        /// </summary>
        /// <param name="ServiceCollection">要查询的服务集合</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoUsbDevice(String[] ServiceCollection)
        {
            if (ServiceCollection == null || ServiceCollection.Length == 0)
                return WhoUsbDevice(UInt16.MinValue, UInt16.MinValue, Guid.Empty);

            List<PnPEntityInfo> UsbDevices = new List<PnPEntityInfo>();

            // 获取USB控制器及其相关联的设备实体
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                String theService = Entity["Service"] as String;          // 服务
                                if (String.IsNullOrEmpty(theService)) continue;

                                foreach (String Service in ServiceCollection)
                                {   // 注意：忽视大小写
                                    if (String.Compare(theService, Service, true) != 0) continue;

                                    PnPEntityInfo Element;
                                    Element.PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                                    Element.Name = Entity["Name"] as String;                // 设备名称
                                    Element.Description = Entity["Description"] as String;  // 设备描述
                                    Element.Service = theService;                           // 服务
                                    Element.Status = Entity["Status"] as String;            // 设备状态
                                    Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识   
                                    Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                                    Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                                    UsbDevices.Add(Element);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (UsbDevices.Count == 0) return null; else return UsbDevices.ToArray();
        }
        #endregion

        #region PnPEntity
        /// <summary>
        /// 所有即插即用设备实体（过滤没有VID和PID的设备）
        /// </summary>
        public static PnPEntityInfo[] AllPnPEntities
        {
            get
            {
                return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, Guid.Empty);
            }
        }

        /// <summary>
        /// 根据VID和PID及设备安装类GUID定位即插即用设备实体
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// HID：{745a17a0-74d3-11d0-b6fe-00a0c90f57da}
        /// Imaging Device：{6bdd1fc6-810f-11d0-bec7-08002be2092f}
        /// Keyboard：{4d36e96b-e325-11ce-bfc1-08002be10318} 
        /// Mouse：{4d36e96f-e325-11ce-bfc1-08002be10318}
        /// Network Adapter：{4d36e972-e325-11ce-bfc1-08002be10318}
        /// USB：{36fc9e60-c465-11cf-8056-444553540000}
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(UInt16 VendorID, UInt16 ProductID, Guid ClassGuid)
        {
            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String VIDPID;
            if (VendorID == UInt16.MinValue)
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID[_]____&PID[_]____%'";
                else
                    VIDPID = "'%VID[_]____&PID[_]" + ProductID.ToString("X4") + "%'";
            }
            else
            {
                if (ProductID == UInt16.MinValue)
                    VIDPID = "'%VID[_]" + VendorID.ToString("X4") + "&PID[_]____%'";
                else
                    VIDPID = "'%VID[_]" + VendorID.ToString("X4") + "&PID[_]" + ProductID.ToString("X4") + "%'";
            }

            String QueryString;
            if (ClassGuid == Guid.Empty)
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID;
            else
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE" + VIDPID + " AND ClassGuid='" + ClassGuid.ToString("B") + "'";

            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String PNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(PNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        PnPEntityInfo Element;

                        Element.PNPDeviceID = PNPDeviceID;                      // 设备ID
                        Element.Name = Entity["Name"] as String;                // 设备名称
                        Element.Description = Entity["Description"] as String;  // 设备描述
                        Element.Service = Entity["Service"] as String;          // 服务
                        Element.Status = Entity["Status"] as String;            // 设备状态
                        Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                        PnPEntities.Add(Element);
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }

        /// <summary>
        /// 根据VID和PID定位即插即用设备实体
        /// </summary>
        /// <param name="VendorID">供应商标识，MinValue忽视</param>
        /// <param name="ProductID">产品编号，MinValue忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoPnPEntity(UInt16 VendorID, UInt16 ProductID)
        {
            return WhoPnPEntity(VendorID, ProductID, Guid.Empty);
        }

        /// <summary>
        /// 根据设备安装类GUID定位即插即用设备实体
        /// </summary>
        /// <param name="ClassGuid">设备安装类Guid，Empty忽视</param>
        /// <returns>设备列表</returns>
        public static PnPEntityInfo[] WhoPnPEntity(Guid ClassGuid)
        {
            return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, ClassGuid);
        }

        /// <summary>
        /// 根据设备ID定位设备
        /// </summary>
        /// <param name="PNPDeviceID">设备ID，可以是不完整信息</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// 注意：对于下划线，需要写成“[_]”，否则视为任意字符
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(String PNPDeviceID)
        {
            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String QueryString;
            if (String.IsNullOrEmpty(PNPDeviceID))
            {
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID[_]____&PID[_]____%'";
            }
            else
            {   // LIKE子句中有反斜杠字符将会引发WQL查询异常
                QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%" + PNPDeviceID.Replace('\\', '_') + "%'";
            }

            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String thePNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(thePNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");


                    PnPEntityInfo Element;

                    Element.PNPDeviceID = thePNPDeviceID;                   // 设备ID
                    Element.Name = Entity["Name"] as String;                // 设备名称
                    Element.Description = Entity["Description"] as String;  // 设备描述
                    Element.Service = Entity["Service"] as String;          // 服务
                    Element.Status = Entity["Status"] as String;            // 设备状态
                    Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                    if (match.Success)
                    {
                        Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                    }
                    else
                    {
                        Element.VendorID = ushort.MinValue;
                        Element.ProductID = ushort.MinValue;
                    }
                    PnPEntities.Add(Element);
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }

        /// <summary>
        /// 根据服务定位设备
        /// </summary>
        /// <param name="ServiceCollection">要查询的服务集合，null忽视</param>
        /// <returns>设备列表</returns>
        /// <remarks>
        /// 跟服务相关的类：
        ///     Win32_SystemDriverPNPEntity
        ///     Win32_SystemDriver
        /// </remarks>
        public static PnPEntityInfo[] WhoPnPEntity(String[] ServiceCollection)
        {
            if (ServiceCollection == null || ServiceCollection.Length == 0)
                return WhoPnPEntity(UInt16.MinValue, UInt16.MinValue, Guid.Empty);

            List<PnPEntityInfo> PnPEntities = new List<PnPEntityInfo>();

            // 枚举即插即用设备实体
            String QueryString = "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID[_]____&PID[_]____%'";
            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher(QueryString).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    String PNPDeviceID = Entity["PNPDeviceID"] as String;
                    Match match = Regex.Match(PNPDeviceID, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        String theService = Entity["Service"] as String;            // 服务
                        if (String.IsNullOrEmpty(theService)) continue;

                        foreach (String Service in ServiceCollection)
                        {   // 注意：忽视大小写
                            if (String.Compare(theService, Service, true) != 0) continue;

                            PnPEntityInfo Element;

                            Element.PNPDeviceID = PNPDeviceID;                      // 设备ID
                            Element.Name = Entity["Name"] as String;                // 设备名称
                            Element.Description = Entity["Description"] as String;  // 设备描述
                            Element.Service = theService;                           // 服务
                            Element.Status = Entity["Status"] as String;            // 设备状态
                            Element.VendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                            Element.ProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                            Element.ClassGuid = new Guid(Entity["ClassGuid"] as String);            // 设备安装类GUID

                            PnPEntities.Add(Element);
                            break;
                        }
                    }
                }
            }

            if (PnPEntities.Count == 0) return null; else return PnPEntities.ToArray();
        }
        #endregion

        #region GetUsbControllerInfo
        public class UsbControllerDevice
        {
            public string Antecedent;

            public string Dependent;
        }
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
        #endregion

        #region GetUsbMoveDiskLogicDrive
        /// <summary>
        /// 得到当前可移动存储设备的盘符
        /// </summary>
        /// <param name="PNPDeviceID"></param>
        /// <returns></returns>
        public static StringCollection WMI_GetDiskRoot(string PNPDeviceID)
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
        /// 得到当前移动存储设备的类型
        /// </summary>
        /// <param name="PNPDeviceID"></param>
        /// <returns></returns>
        public static DriveType WMI_GetDiskType(string PNPDeviceID)
        {
            DriveType driveType=default(DriveType);
            var list = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE PNPDeviceID='" + PNPDeviceID + "'").Get();
            foreach (ManagementObject item in new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE PNPDeviceID='" + PNPDeviceID + "'").Get())
            {
                foreach (ManagementObject item2 in item.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject item3 in item2.GetRelated("Win32_LogicalDisk"))
                    {
                        driveType = item3["DriveType"]==null? default(DriveType) : (DriveType)Enum.Parse(typeof(DriveType), item3["DriveType"].ToString());
                    }
                }
            }
            return driveType;
        }
        #endregion
    }
}
