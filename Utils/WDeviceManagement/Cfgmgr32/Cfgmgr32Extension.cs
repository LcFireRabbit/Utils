using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WDeviceManagement.Cfgmgr32
{
    public class Cfgmgr32Extension:Cfgmgr32Base
    {
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
