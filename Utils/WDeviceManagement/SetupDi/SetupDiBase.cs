using System;
using System.Runtime.InteropServices;

namespace Utils.WDeviceManagement.SetupDi
{
    public class SetupDiBase
    {
        private const string SETUPAPI = "setupapi.dll";
        internal const uint ERROR_INVALID_DATA = 13;
        internal const uint ERROR_NO_MORE_ITEMS = 259;
        internal const uint ERROR_ELEMENT_NOT_FOUND = 1168;

        #region Enumerations

        [Flags]
        internal enum DIGCF : uint
        {
            DEFAULT = 0x00000001,
            PRESENT = 0x00000002,
            ALLCLASSES = 0x00000004,
            PROFILE = 0x00000008,
            DEVICEINTERFACE = 0x00000010
        }

        internal enum SPDRP : uint
        {
            /// <summary>
            /// DeviceDesc (R/W)
            /// </summary>
            DEVICEDESC = 0x00000000,

            /// <summary>
            /// HardwareID (R/W)
            /// </summary>
            HARDWAREID = 0x00000001,

            /// <summary>
            /// CompatibleIDs (R/W)
            /// </summary>
            COMPATIBLEIDS = 0x00000002,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED0 = 0x00000003,

            /// <summary>
            /// Service (R/W)
            /// </summary>
            SERVICE = 0x00000004,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED1 = 0x00000005,

            /// <summary>
            /// unused
            /// </summary>
            UNUSED2 = 0x00000006,

            /// <summary>
            /// Class (R--tied to ClassGUID)
            /// </summary>
            CLASS = 0x00000007,

            /// <summary>
            /// ClassGUID (R/W)
            /// </summary>
            CLASSGUID = 0x00000008,

            /// <summary>
            /// Driver (R/W)
            /// </summary>
            DRIVER = 0x00000009,

            /// <summary>
            /// ConfigFlags (R/W)
            /// </summary>
            CONFIGFLAGS = 0x0000000A,

            /// <summary>
            /// Mfg (R/W)
            /// </summary>
            MFG = 0x0000000B,

            /// <summary>
            /// FriendlyName (R/W)
            /// </summary>
            FRIENDLYNAME = 0x0000000C,

            /// <summary>
            /// LocationInformation (R/W)
            /// </summary>
            LOCATION_INFORMATION = 0x0000000D,

            /// <summary>
            /// PhysicalDeviceObjectName (R)
            /// </summary>
            PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,

            /// <summary>
            /// Capabilities (R)
            /// </summary>
            CAPABILITIES = 0x0000000F,

            /// <summary>
            /// UiNumber (R)
            /// </summary>
            UI_NUMBER = 0x00000010,

            /// <summary>
            /// UpperFilters (R/W)
            /// </summary>
            UPPERFILTERS = 0x00000011,

            /// <summary>
            /// LowerFilters (R/W)
            /// </summary>
            LOWERFILTERS = 0x00000012,

            /// <summary>
            /// BusTypeGUID (R)
            /// </summary>
            BUSTYPEGUID = 0x00000013,

            /// <summary>
            /// LegacyBusType (R)
            /// </summary>
            LEGACYBUSTYPE = 0x00000014,

            /// <summary>
            /// BusNumber (R)
            /// </summary>
            BUSNUMBER = 0x00000015,

            /// <summary>
            /// Enumerator Name (R)
            /// </summary>
            ENUMERATOR_NAME = 0x00000016,

            /// <summary>
            /// Security (R/W, binary form)
            /// </summary>
            SECURITY = 0x00000017,

            /// <summary>
            /// Security (W, SDS form)
            /// </summary>
            SECURITY_SDS = 0x00000018,

            /// <summary>
            /// Device Type (R/W)
            /// </summary>
            DEVTYPE = 0x00000019,

            /// <summary>
            /// Device is exclusive-access (R/W)
            /// </summary>
            EXCLUSIVE = 0x0000001A,

            /// <summary>
            /// Device Characteristics (R/W)
            /// </summary>
            CHARACTERISTICS = 0x0000001B,

            /// <summary>
            /// Device Address (R)
            /// </summary>
            ADDRESS = 0x0000001C,

            /// <summary>
            /// UiNumberDescFormat (R/W)
            /// </summary>
            UI_NUMBER_DESC_FORMAT = 0X0000001D,

            /// <summary>
            /// Device Power Data (R)
            /// </summary>
            DEVICE_POWER_DATA = 0x0000001E,

            /// <summary>
            /// Removal Policy (R)
            /// </summary>
            REMOVAL_POLICY = 0x0000001F,

            /// <summary>
            /// Hardware Removal Policy (R)
            /// </summary>
            REMOVAL_POLICY_HW_DEFAULT = 0x00000020,

            /// <summary>
            /// Removal Policy Override (RW)
            /// </summary>
            REMOVAL_POLICY_OVERRIDE = 0x00000021,

            /// <summary>
            /// Device Install State (R)
            /// </summary>
            INSTALL_STATE = 0x00000022,

            /// <summary>
            /// Device Location Paths (R)
            /// </summary>
            LOCATION_PATHS = 0x00000023,
        }

        internal enum DIF : uint
        {
            SELECTDEVICE = 0x00000001,
            INSTALLDEVICE = 0x00000002,
            ASSIGNRESOURCES = 0x00000003,
            PROPERTIES = 0x00000004,
            REMOVE = 0x00000005,
            FIRSTTIMESETUP = 0x00000006,
            FOUNDDEVICE = 0x00000007,
            SELECTCLASSDRIVERS = 0x00000008,
            VALIDATECLASSDRIVERS = 0x00000009,
            INSTALLCLASSDRIVERS = 0x0000000A,
            CALCDISKSPACE = 0x0000000B,
            DESTROYPRIVATEDATA = 0x0000000C,
            VALIDATEDRIVER = 0x0000000D,
            DETECT = 0x0000000F,
            INSTALLWIZARD = 0x00000010,
            DESTROYWIZARDDATA = 0x00000011,
            PROPERTYCHANGE = 0x00000012,
            ENABLECLASS = 0x00000013,
            DETECTVERIFY = 0x00000014,
            INSTALLDEVICEFILES = 0x00000015,
            UNREMOVE = 0x00000016,
            SELECTBESTCOMPATDRV = 0x00000017,
            ALLOW_INSTALL = 0x00000018,
            REGISTERDEVICE = 0x00000019,
            NEWDEVICEWIZARD_PRESELECT = 0x0000001A,
            NEWDEVICEWIZARD_SELECT = 0x0000001B,
            NEWDEVICEWIZARD_PREANALYZE = 0x0000001C,
            NEWDEVICEWIZARD_POSTANALYZE = 0x0000001D,
            NEWDEVICEWIZARD_FINISHINSTALL = 0x0000001E,
            UNUSED1 = 0x0000001F,
            INSTALLINTERFACES = 0x00000020,
            DETECTCANCEL = 0x00000021,
            REGISTER_COINSTALLERS = 0x00000022,
            ADDPROPERTYPAGE_ADVANCED = 0x00000023,
            ADDPROPERTYPAGE_BASIC = 0x00000024,
            RESERVED1 = 0x00000025,
            TROUBLESHOOTER = 0x00000026,
            POWERMESSAGEWAKE = 0x00000027,
            ADDREMOTEPROPERTYPAGE_ADVANCED = 0x00000028,
            UPDATEDRIVER_UI = 0x00000029,
            FINISHINSTALL_ACTION = 0x0000002A,
            RESERVED2 = 0x00000030,
        }

        internal enum DICS : uint
        {
            ENABLE = 0x00000001,
            DISABLE = 0x00000002,
            PROPCHANGE = 0x00000003,
            START = 0x00000004,
            STOP = 0x00000005,
        }

        [Flags]
        internal enum DICS_FLAG : uint
        {
            GLOBAL = 0x00000001,
            CONFIGSPECIFIC = 0x00000002,
            CONFIGGENERAL = 0x00000004,
        }


        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public DIF InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public DICS StateChange;
            public DICS_FLAG Scope;
            public UInt32 HwProfile;
        }

        #endregion


        #region P/Invoke Functions

        ///api文档地址https://docs.microsoft.com/zh-cn/windows/win32/api/setupapi/

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevsW([In] ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPWStr)]string Enumerator, IntPtr parent, DIGCF flags);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, UInt32 memberIndex, [Out] out SP_DEVINFO_DATA deviceInfoData);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiSetClassInstallParams(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData,
            [In] ref SP_PROPCHANGE_PARAMS classInstallParams, UInt32 ClassInstallParamsSize);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiChangeState(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiRemoveDevice(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport(SETUPAPI, SetLastError = true)]
        internal static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property,
            [Out] out UInt32 PropertyRegDataType, IntPtr PropertyBuffer, UInt32 PropertyBufferSize, [In, Out] ref UInt32 RequiredSize);

        [DllImport(SETUPAPI, CharSet = CharSet.Auto)]
        internal static extern bool SetupDiCallClassInstaller(UInt32 flags, IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);

        #endregion
    }
}
