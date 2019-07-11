using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils.WDeviceManagement.SetupDi
{
    public class SetupDiExtension : SetupDiBase
    {
        #region DeviceClassGuid

        /// <summary>
        /// HID
        /// </summary>
        private static readonly string HIDDevice = "{745a17a0-74d3-11d0-b6fe-00a0c90f57da}";

        /// <summary>
        /// Keyboard
        /// </summary>
        private static readonly string Keyboard = "{4d36e96b-e325-11ce-bfc1-08002be10318}";

        /// <summary>
        /// Mouse
        /// </summary>
        private static readonly string Mouse = "{4d36e96f-e325-11ce-bfc1-08002be10318}";

        /// <summary>
        /// USB
        /// </summary>
        private static readonly string USB = "{36fc9e60-c465-11cf-8056-444553540000}";

        #endregion

        /// <summary>
        /// 禁用鼠标类设备,如果不启用,鼠标类设备将不可用。
        /// </summary>
        public static void DisableMouse()
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    if (devdata.ClassGuid == new Guid(Mouse))
                    {
                        SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                        header.cbSize = (UInt32)Marshal.SizeOf(header);
                        header.InstallFunction = DIF.PROPERTYCHANGE;

                        SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                        {
                            ClassInstallHeader = header,
                            StateChange = DICS.DISABLE,
                            Scope = DICS_FLAG.GLOBAL,
                            HwProfile = 0
                        };

                        SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));

                        SetupDiChangeState(info, ref devdata);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ChangeMouseState failed,the reason is {0}", ex.Message));
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }

        /// <summary>
        /// 启用鼠标类设备
        /// </summary>
        public static void EnableMouse()
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    if (devdata.ClassGuid == new Guid(Mouse))
                    {
                        SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                        header.cbSize = (UInt32)Marshal.SizeOf(header);
                        header.InstallFunction = DIF.PROPERTYCHANGE;

                        SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                        {
                            ClassInstallHeader = header,
                            StateChange = DICS.ENABLE,
                            Scope = DICS_FLAG.GLOBAL,
                            HwProfile = 0
                        };

                        SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));

                        SetupDiChangeState(info, ref devdata);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("ChangeMouseState failed,the reason is {0}", ex.Message));
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }

        /// <summary>
        /// 通过设备唯一标识PnpDeviceID,禁用设备,前提设备可被禁用
        /// </summary>
        /// <param name="pnpDeviceId"></param>
        /// <returns></returns>
        public static bool DisableDeviceByPnpDeviceId(string pnpDeviceId)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            bool isSuccess = false;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, pnpDeviceId, IntPtr.Zero, DIGCF.DEVICEINTERFACE);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    int size = 512;
                    StringBuilder stringBuilder = new StringBuilder(size);
                    uint result = Cfgmgr32.Cfgmgr32Base.CM_Get_Device_ID(devdata.DevInst, stringBuilder, size);
                    if (result == 0)
                    {
                        string currentPnpDeviceID = stringBuilder.ToString();

                        if (currentPnpDeviceID == pnpDeviceId)
                        {
                            SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                            header.cbSize = (UInt32)Marshal.SizeOf(header);
                            header.InstallFunction = DIF.PROPERTYCHANGE;

                            SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                            {
                                ClassInstallHeader = header,
                                StateChange = DICS.DISABLE,
                                Scope = DICS_FLAG.GLOBAL,
                                HwProfile = 0
                            };

                            SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));

                            SetupDiChangeState(info, ref devdata);

                            isSuccess = true;
                        }
                    }
                    continue;
                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("DisableDeviceByPnpDeviceId failed,the reason is {0}", ex.Message));

                return isSuccess;
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }

        /// <summary>
        /// 通过设备唯一标识PnpDeviceID,启用设备
        /// </summary>
        /// <param name="pnpDeviceId"></param>
        /// <returns></returns>
        public static bool EnableDeviceByPnpDeviceId(string pnpDeviceId)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            bool isSuccess = false;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, pnpDeviceId, IntPtr.Zero, DIGCF.DEVICEINTERFACE);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    int size = 512;
                    StringBuilder stringBuilder = new StringBuilder(size);
                    uint result = Cfgmgr32.Cfgmgr32Base.CM_Get_Device_ID(devdata.DevInst, stringBuilder, size);
                    if (result == 0)
                    {
                        string currentPnpDeviceID = stringBuilder.ToString();

                        if (currentPnpDeviceID == pnpDeviceId)
                        {
                            SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
                            header.cbSize = (UInt32)Marshal.SizeOf(header);
                            header.InstallFunction = DIF.PROPERTYCHANGE;

                            SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS
                            {
                                ClassInstallHeader = header,
                                StateChange = DICS.ENABLE,
                                Scope = DICS_FLAG.GLOBAL,
                                HwProfile = 0
                            };

                            SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));

                            SetupDiChangeState(info, ref devdata);

                            isSuccess = true;
                        }
                    }
                    continue;
                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("EnableDeviceByPnpDeviceId failed,the reason is {0}", ex.Message));

                return isSuccess;
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }
        
        /// <summary>
        /// 卸载键盘
        /// </summary>
        public static void UnloadKeyboard()
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    if (devdata.ClassGuid == new Guid(Keyboard))
                    {
                        SetupDiRemoveDevice(info, ref devdata);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("UnloadKeyboard failed,the reason is {0}", ex.Message));
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }

        /// <summary>
        /// 通过设备唯一标识PnpDeviceID,卸载设备
        /// </summary>
        /// <param name="pnpDeviceId"></param>
        /// <returns></returns>
        public static bool UnloadDeviceByPnpDeviceId(string pnpDeviceId)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            bool isSuccess = false;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF.ALLCLASSES);

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

                ///遍历设备
                for (uint i = 0; SetupDiEnumDeviceInfo(info, i, out devdata); i++)
                {
                    int size = 512;
                    StringBuilder stringBuilder = new StringBuilder(size);
                    uint result = Cfgmgr32.Cfgmgr32Base.CM_Get_Device_ID(devdata.DevInst, stringBuilder, size);
                    if (result == 0)
                    {
                        string currentPnpDeviceID = stringBuilder.ToString();

                        if (currentPnpDeviceID == pnpDeviceId)
                        {
                            SetupDiRemoveDevice(info, ref devdata);

                            isSuccess = true;
                        }
                    }
                    continue;
                }
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("UnloadDeviceByPnpDeviceId failed,the reason is {0}", ex.Message));
                return isSuccess;
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }

        }

        /// <summary>
        /// 扫描检测硬件改动，重新加载已卸载设备
        /// </summary>
        public static void ScanForHardWareChanges()
        {
            try
            {
                uint pdnDevInst = 0u;
                Cfgmgr32.Cfgmgr32Base.CM_Locate_DevNodeA(ref pdnDevInst, null);
                Cfgmgr32.Cfgmgr32Base.CM_Reenumerate_DevNode(pdnDevInst);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("UnloadDeviceByPnpDeviceId failed,the reason is {0}", ex.Message));
            }
        }

        /// <summary>
        /// 得到设备某项属性的值
        /// </summary>
        /// <param name="info"></param>
        /// <param name="devdata"></param>
        /// <param name="propId"></param>
        /// <returns></returns>
        private static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, SPDRP propId)
        {
            uint outsize;
            IntPtr buffer = IntPtr.Zero;
            try
            {
                uint buflen = 512;
                buffer = Marshal.AllocHGlobal((int)buflen);
                outsize = 0;
                // CHANGE #2 - Use this instead of SetupDiGetDeviceProperty 
                SetupDiGetDeviceRegistryPropertyW(info, ref devdata, propId, out uint proptype, buffer, buflen, ref outsize);
                byte[] lbuffer = new byte[outsize];
                Marshal.Copy(buffer, lbuffer, 0, (int)outsize);
                int errcode = Marshal.GetLastWin32Error();
                if (errcode == ERROR_INVALID_DATA) return null;
                return Encoding.Unicode.GetString(lbuffer);
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
