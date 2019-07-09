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

        #region Enum
        public enum State
        {
            DISABLE,
            ENABLE
        }
        #endregion

        /// <summary>
        /// 是否禁用鼠标
        /// </summary>
        /// <param name="state"></param>
        public static void ChangeMouseState(State mouseState)
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
                            StateChange = mouseState == State.DISABLE ? DICS.DISABLE : DICS.ENABLE,
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
        /// 卸载键盘
        /// </summary>
        public static void UnloadKeyboard()
        {

        }

        //禁用指定设备
        public static void ChangeDevieState(string Enumerator)
        {
            IntPtr info = IntPtr.Zero;
            Guid NullGuid = Guid.Empty;
            try
            {
                info = SetupDiGetClassDevsW(ref NullGuid, Enumerator, IntPtr.Zero, DIGCF.ALLCLASSES);

                if(!ReferenceEquals(info,IntPtr.Zero))
                {

                }

                SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
                devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (info != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(info);
            }
        }

        //卸载指定设备


        /// <summary>
        /// 得到设备的友好名
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
