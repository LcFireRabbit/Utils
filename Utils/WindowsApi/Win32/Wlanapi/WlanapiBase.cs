using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public partial class WlanapiBase
    {
        private const string Wlanapi = "Wlanapi.dll";

        public const uint WLAN_CLIENT_VERSION_XP_SP2 = 1;
        public const uint WLAN_CLIENT_VERSION_LONGHORN = 2;

        /// <summary>
		/// Defines the callback function which accepts WLAN notifications.
		/// </summary>
		public delegate void WlanNotificationCallbackDelegate(ref WlanNotificationData notificationData, IntPtr context);

        //https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanopenhandle
        [DllImport(Wlanapi)]
        public static extern int WlanOpenHandle(
			uint dwClientVersion,
			IntPtr pReserved, 
			[Out] out uint pdwNegotiatedVersion, 
			[Out] out IntPtr ClientHandle);

		//https://learn.microsoft.com/zh-cn/previous-versions/windows/embedded/gg159537(v=winembedded.80)
		[DllImport(Wlanapi)]
		public static extern int WlanRegisterNotification(
			IntPtr hClientHandle,
			WlanNotificationSource dwNotifSource,
			bool bIgnoreDuplicate,
			WlanNotificationCallbackDelegate funcCallback,
			IntPtr pCallbackContext,
			IntPtr pReserved,
			[Out] out WlanNotificationSource pdwPrevNotifSource);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanclosehandle
		[DllImport(Wlanapi)]
		public static extern int WlanCloseHandle(
			[In] IntPtr hClientHandle, 
			IntPtr pReserved);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanqueryinterface
		[DllImport(Wlanapi)]
		public static extern int WlanQueryInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode, 
			[In, Out] IntPtr pReserved,
			[Out] out int dataSize,
			[Out] out IntPtr ppData,
			[Out] out WlanOpcodeValueType wlanOpcodeValueType);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlansetinterface
		[DllImport(Wlanapi)]
		public static extern int WlanSetInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode,
			[In] uint dataSize,
			[In] IntPtr pData,
			[In, Out] IntPtr pReserved);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanscan
		/// <param name="pDot11Ssid">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		/// <param name="pIeData">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		[DllImport(Wlanapi)]
		public static extern int WlanScan(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pDot11Ssid,
			[In] IntPtr pIeData,
			[In, Out] IntPtr pReserved);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanconnect
		[DllImport(Wlanapi)]
		public static extern int WlanConnect(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] ref WlanConnectionParameters connectionParameters,
			IntPtr pReserved);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlandisconnect
		[DllImport(Wlanapi)]
		public static extern int WlanDisconnect(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			IntPtr pReserved);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlangetprofile
		/// <param name="flags">Not supported on Windows XP SP2: must be a <c>null</c> reference.</param>
		[DllImport(Wlanapi)]
		public static extern int WlanGetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileXml,
			[Out, Optional] out WlanProfileFlags flags,
			[Out, Optional] out WlanAccess grantedAccess);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlangetprofilelist
		[DllImport(Wlanapi)]
		public static extern int WlanGetProfileList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileList
		);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlangetavailablenetworklist
		[DllImport(Wlanapi)]
		public static extern int WlanGetAvailableNetworkList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanGetAvailableNetworkFlags flags,
			[In, Out] IntPtr reservedPtr,
			[Out] out IntPtr availableNetworkListPtr);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlangetnetworkbsslist
		[DllImport(Wlanapi)]
		public static extern int WlanGetNetworkBssList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr dot11SsidInt,
			[In] Dot11BssType dot11BssType,
			[In] bool securityEnabled,
			IntPtr reservedPtr,
			[Out] out IntPtr wlanBssList
		);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofile
		[DllImport(Wlanapi)]
		public static extern int WlanSetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanProfileFlags flags,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileXml,
			[In, Optional, MarshalAs(UnmanagedType.LPWStr)] string allUserProfileSecurity,
			[In] bool overwrite,
			[In] IntPtr pReserved,
			[Out] out WlanReasonCode reasonCode);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlandeleteprofile
		[DllImport(Wlanapi)]
		public static extern int WlanDeleteProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			IntPtr reservedPtr
		);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlansetprofileeapxmluserdata
		[DllImport(Wlanapi)]
		public static extern int WlanSetProfileEapXmlUserData(
			// The client's session handle, obtained by a previous call to the WlanOpenHandle function.
			// 客户端的会话句柄，通过先前对WlanOpenHandle函数的调用获得。
			[In] IntPtr clientHandle,
			// The GUID of the interface.
			// 接口的GUID。
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			// The name of the profile associated with the EAP user data.
			// 与EAP用户数据关联的配置文件的名称。
			// Profile names are case-sensitive. This string must be NULL-terminated.
			// 配置文件名称区分大小写。此字符串必须以NULL结尾。
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			// A set of flags that modify the behavior of the function.
			[In] SetEapUserDataMode dwFlags,
			// A pointer to XML data used to set the user credentials,
			// 指向用于设置用户凭证的XML数据的指针，
			// The XML data must be based on the EAPHost User Credentials schema. To view sample user credential XML data,
			// XML数据必须基于EAPHost用户凭据架构。要查看示例用户凭证XML数据，
			// see EAPHost User Properties: http://msdn.microsoft.com/en-us/library/windows/desktop/bb204765(v=vs.85).aspx
			[In, MarshalAs(UnmanagedType.LPWStr)] string userDataXML,   
			IntPtr reservedPtr
		);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanreasoncodetostring
		[DllImport(Wlanapi)]
		public static extern int WlanReasonCodeToString(
			[In] WlanReasonCode reasonCode,
			[In] int bufferSize,
			[In, Out] StringBuilder stringBuffer,
			IntPtr pReserved
		);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanenuminterfaces
		[DllImport(Wlanapi)]
		public static extern int WlanEnumInterfaces(
			[In] IntPtr clientHandle,
			[In, Out] IntPtr pReserved,
			[Out] out IntPtr ppInterfaceList);

		//https://learn.microsoft.com/zh-cn/windows/win32/api/wlanapi/nf-wlanapi-wlanfreememory
		[DllImport(Wlanapi)]
		public static extern void WlanFreeMemory(IntPtr pMemory);

		/// <summary>
		/// Helper method to wrap calls to Native WiFi API methods.
		/// If the method falls, throws an exception containing the error code.
		/// </summary>
		/// <param name="win32ErrorCode">The error code.</param>
		[DebuggerStepThrough]
		internal static void ThrowIfError(int win32ErrorCode)
		{
			if (win32ErrorCode != 0)
				throw new Win32Exception(win32ErrorCode);
		}
	}
}
