using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils.WDeviceManagement.Cfgmgr32
{
    public class Cfgmgr32Base
    {
        private const string Cfgmgr32 = "Cfgmgr32.dll";
        
        public enum CM_LOCATE_DEVNODE_FLAG : uint
        {
            CM_LOCATE_DEVNODE_NORMAL = 0u,
            CM_LOCATE_DEVNODE_PHANTOM = 1u,
            CM_LOCATE_DEVNODE_CANCELREMOVE = 2u,
            CM_LOCATE_DEVNODE_NOVALIDATION = 4u,
            CM_LOCATE_DEVNODE_BITS = 7u
        }

        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, int ulFlags = 0);
        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Device_ID(uint dnDevInst, [Out] StringBuilder Buffer, int BufferLen, int ulFlags = 0);
        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern uint CM_Locate_DevNode(out uint pdnDevInst, string pDeviceID, CM_LOCATE_DEVNODE_FLAG ulFlags = CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_NORMAL);
        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_DevNode_Registry_Property(uint dnDevInst, uint ulProperty, IntPtr pulRegDataType, ref int Buffer, ref int pulLength, uint ulFlags = 0u);

        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern int CM_Locate_DevNodeA(ref uint pdnDevInst, string pDeviceID, uint ulFlags=0u);
        [DllImport(Cfgmgr32, CharSet = CharSet.Unicode)]
        public static extern int CM_Reenumerate_DevNode(uint dnDevInst, uint ulFlags = 0u);
    }
}
