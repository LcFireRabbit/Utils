using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class WlanInterface
	{
        #region Fields
		/// <summary>
		/// 网卡管理器
		/// </summary>
        private WlanManager _manager;
		/// <summary>
		/// WLAN网卡信息
		/// </summary>
        private WlanInterfaceInfo _info;

		private bool _queueEvents;

		private AutoResetEvent _eventQueueFilled = new AutoResetEvent(false);

		private Queue<object> _eventQueue = new Queue<object>();
		#endregion

		#region Events
		/// <summary>
		/// Represents a method that will handle <see cref="WlanNotification"/> events.
		/// </summary>
		/// <param name="notifyData">The notification data.</param>
		public delegate void WlanNotificationEventHandler(WlanNotificationData notifyData);

		/// <summary>
		/// Represents a method that will handle <see cref="WlanConnectionNotification"/> events.
		/// </summary>
		/// <param name="notifyData">The notification data.</param>
		/// <param name="connNotifyData">The notification data.</param>
		public delegate void WlanConnectionNotificationEventHandler(WlanNotificationData notifyData, WlanConnectionNotificationData connNotifyData);

		/// <summary>
		/// Represents a method that will handle <see cref="WlanReasonNotification"/> events.
		/// </summary>
		/// <param name="notifyData">The notification data.</param>
		/// <param name="reasonCode">The reason code.</param>
		public delegate void WlanReasonNotificationEventHandler(WlanNotificationData notifyData, WlanReasonCode reasonCode);

		/// <summary>
		/// Occurs when an event of any kind occurs on a WLAN interface.
		/// </summary>
		public event WlanNotificationEventHandler WlanNotification;

		/// <summary>
		/// Occurs when a WLAN interface changes connection state.
		/// </summary>
		public event WlanConnectionNotificationEventHandler WlanConnectionNotification;

		/// <summary>
		/// Occurs when a WLAN operation fails due to some reason.
		/// </summary>
		public event WlanReasonNotificationEventHandler WlanReasonNotification;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="WlanAdapter"/> is automatically configured.
		/// </summary>
		/// <value><c>true</c> if "autoconf" is enabled; otherwise, <c>false</c>.</value>
		public bool AutoConfig
		{
			get
			{
				return GetInterfaceInt(WlanIntfOpcode.AutoconfEnabled) != 0;
			}
			set
			{
				SetInterfaceInt(WlanIntfOpcode.AutoconfEnabled, value ? 1 : 0);
			}
		}

		/// <summary>
		/// Gets the network interface of this wireless interface.
		/// </summary>
		/// <remarks>
		/// The network interface allows querying of generic network properties such as the interface's IP address.
		/// </remarks>
		public NetworkInterface NetworkInterface
		{
			get
			{
				// Do not cache the NetworkInterface; We need it fresh
				// each time cause otherwise it caches the IP information.
				foreach (NetworkInterface netIface in NetworkInterface.GetAllNetworkInterfaces())
				{
					Guid netIfaceGuid = new Guid(netIface.Id);
					if (netIfaceGuid.Equals(_info.InterfaceGuid))
					{
						return netIface;
					}
				}
				return null;
			}
		}

		/// <summary>
		/// The GUID of the interface (same content as the <see cref="System.Net.NetworkInformation.NetworkInterface.Id"/> value).
		/// </summary>
		public Guid InterfaceGuid
		{
			get { return _info.InterfaceGuid; }
		}

		/// <summary>
		/// The description of the interface.
		/// This is a user-immutable string containing the vendor and model name of the adapter.
		/// </summary>
		public string InterfaceDescription
		{
			get { return _info.InterfaceDescription; }
		}

		/// <summary>
		/// The friendly name given to the interface by the user (e.g. "Local Area Network Connection").
		/// </summary>
		public string InterfaceName
		{
			get { return NetworkInterface.Name; }
		}

		/// <summary>
		/// Gets or sets the BSS type for the indicated interface.
		/// </summary>
		/// <value>The type of the BSS.</value>
		public Dot11BssType BssType
		{
			get
			{
				return (Dot11BssType)GetInterfaceInt(WlanIntfOpcode.BssType);
			}
			set
			{
				SetInterfaceInt(WlanIntfOpcode.BssType, (int)value);
			}
		}

		/// <summary>
		/// Gets the state of the interface.
		/// </summary>
		/// <value>The state of the interface.</value>
		public WlanInterfaceState InterfaceState
		{
			get
			{
				return (WlanInterfaceState)GetInterfaceInt(WlanIntfOpcode.InterfaceState);
			}
		}

		/// <summary>
		/// Gets the channel.
		/// </summary>
		/// <value>The channel.</value>
		/// <remarks>Not supported on Windows XP SP2.</remarks>
		public int Channel
		{
			get
			{
				return GetInterfaceInt(WlanIntfOpcode.ChannelNumber);
			}
		}

		/// <summary>
		/// Gets the RSSI.
		/// </summary>
		/// <value>The RSSI.</value>
		/// <remarks>Not supported on Windows XP SP2.</remarks>
		public int RSSI
		{
			get
			{
				return GetInterfaceInt(WlanIntfOpcode.RSSI);
			}
		}

		/// <summary>
		/// Gets the current operation mode.
		/// </summary>
		/// <value>The current operation mode.</value>
		/// <remarks>Not supported on Windows XP SP2.</remarks>
		public Dot11OperationMode CurrentOperationMode
		{
			get
			{
				return (Dot11OperationMode)GetInterfaceInt(WlanIntfOpcode.CurrentOperationMode);
			}
		}

		/// <summary>
		/// Gets the attributes of the current connection.
		/// </summary>
		/// <value>The current connection attributes.</value>
		/// <exception cref="Win32Exception">An exception with code 0x0000139F (The group or resource is not in the correct state to perform the requested operation.) will be thrown if the interface is not connected to a network.</exception>
		public WlanConnectionAttributes CurrentConnection
		{
			get
			{
                //TODO: Should get result from WlanInterop.WlanQueryInterface and handle if it's 0x0000139F (not connected) gracefully
                WlanapiBase.ThrowIfError(WlanapiBase.WlanQueryInterface(_manager._clientHandle, _info.InterfaceGuid, WlanIntfOpcode.CurrentConnection, IntPtr.Zero, out int valueSize, out IntPtr valuePtr, out WlanOpcodeValueType opcodeValueType));
                try
				{
					return (WlanConnectionAttributes)Marshal.PtrToStructure(valuePtr, typeof(WlanConnectionAttributes));
				}
				finally
				{
					WlanapiBase.WlanFreeMemory(valuePtr);
				}
			}
		}
		#endregion

		#region Constructor
		internal WlanInterface(WlanManager client, WlanInterfaceInfo info)
        {
            this._manager = client;
            this._info = info;
        }
		#endregion

		#region Fnc
		/// <summary>
		/// Requests a scan for available networks.
		/// 请求扫描可用网络
		/// </summary>
		/// <remarks>
		/// The method returns immediately. Progress is reported through the <see cref="WlanNotification"/> event.
		/// 该方法立即返回。通过<see cref=“WlanNotification”/>事件报告进度。
		/// </remarks>
		public void Scan()
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanScan(_manager._clientHandle, _info.InterfaceGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
		}

		/// <summary>
		/// Converts a pointer to a available networks list (header + entries) to an array of available network entries.
		/// 将指向可用网络列表（标头+条目）的指针转换为可用网络条目的数组。
		/// </summary>
		/// <param name="bssListPtr">A pointer to an available networks list's header.</param>
		/// <returns>An array of available network entries.</returns>
		private WlanAvailableNetwork[] ConvertAvailableNetworkListPtr(IntPtr availNetListPtr)
		{
			WlanAvailableNetworkListHeader availNetListHeader = (WlanAvailableNetworkListHeader)Marshal.PtrToStructure(availNetListPtr, typeof(WlanAvailableNetworkListHeader));
			long availNetListIt = availNetListPtr.ToInt64() + Marshal.SizeOf(typeof(WlanAvailableNetworkListHeader));
			WlanAvailableNetwork[] availNets = new WlanAvailableNetwork[availNetListHeader.NumberOfItems];
			for (int i = 0; i < availNetListHeader.NumberOfItems; ++i)
			{
				availNets[i] = (WlanAvailableNetwork)Marshal.PtrToStructure(new IntPtr(availNetListIt), typeof(WlanAvailableNetwork));
				availNetListIt += Marshal.SizeOf(typeof(WlanAvailableNetwork));
			}

			return availNets;
		}

		/// <summary>
		/// Retrieves the list of available networks.
		/// 检索可用网络的列表。
		/// </summary>
		/// <param name="flags">Controls the type of networks returned.</param>
		/// <returns>A list of the available networks.</returns>
		public WlanAvailableNetwork[] GetAvailableNetworkList(WlanGetAvailableNetworkFlags flags)
		{
			try
			{
				Scan();

				IntPtr availNetListPtr;

				WlanapiBase.ThrowIfError(WlanapiBase.WlanGetAvailableNetworkList(_manager._clientHandle, _info.InterfaceGuid, flags, IntPtr.Zero, out availNetListPtr));

				var list = ConvertAvailableNetworkListPtr(availNetListPtr);

				WlanapiBase.WlanFreeMemory(availNetListPtr);

				return list;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		/// <summary>
		/// Converts a pointer to a BSS list (header + entries) to an array of BSS entries.
		/// 将指向BSS列表（标头+条目）的指针转换为BSS条目数组。
		/// </summary>
		/// <param name="bssListPtr">A pointer to a BSS list's header.</param>
		/// <returns>An array of BSS entries.</returns>
		private WlanBssEntry[] ConvertBssListPtr(IntPtr bssListPtr)
		{
			WlanBssListHeader bssListHeader = (WlanBssListHeader)Marshal.PtrToStructure(bssListPtr, typeof(WlanBssListHeader));
			long bssListIt = bssListPtr.ToInt64() + Marshal.SizeOf(typeof(WlanBssListHeader));
			WlanBssEntry[] bssEntries = new WlanBssEntry[bssListHeader.NumberOfItems];
			for (int i = 0; i < bssListHeader.NumberOfItems; ++i)
			{
				bssEntries[i] = (WlanBssEntry)Marshal.PtrToStructure(new IntPtr(bssListIt), typeof(WlanBssEntry));
				bssListIt += Marshal.SizeOf(typeof(WlanBssEntry));
			}
			return bssEntries;
		}

		/// <summary>
		/// Retrieves the basic service sets (BSS) list of all available networks.
		/// 检索所有可用网络的基本服务集（BSS）列表。
		/// 介绍基本基本服务集的百科
		/// https://baike.baidu.com/item/%E5%9F%BA%E6%9C%AC%E6%9C%8D%E5%8A%A1%E9%9B%86/10907357?fr=aladdin
		/// </summary>
		public WlanBssEntry[] GetNetworkBssList()
		{
			IntPtr bssListPtr;
			WlanapiBase.ThrowIfError(WlanapiBase.WlanGetNetworkBssList(_manager._clientHandle, _info.InterfaceGuid, IntPtr.Zero, Dot11BssType.Any, false, IntPtr.Zero, out bssListPtr));

			try
			{
				return ConvertBssListPtr(bssListPtr);
			}
			finally
			{
				WlanapiBase.WlanFreeMemory(bssListPtr);
			}
		}

		/// <summary>
		/// Retrieves the basic service sets (BSS) list of the specified network.
		/// 检索指定网络的基本服务集（BSS）列表。
		/// </summary>
		/// <param name="ssid">Specifies the SSID of the network from which the BSS list is requested.</param>
		/// <param name="bssType">Indicates the BSS type of the network.</param>
		/// <param name="securityEnabled">Indicates whether security is enabled on the network.</param>
		public WlanBssEntry[] GetNetworkBssList(Dot11Ssid ssid, Dot11BssType bssType, bool securityEnabled)
		{
			IntPtr ssidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ssid));
			Marshal.StructureToPtr(ssid, ssidPtr, false);

			try
			{
				IntPtr bssListPtr;
				WlanapiBase.ThrowIfError(WlanapiBase.WlanGetNetworkBssList(_manager._clientHandle, _info.InterfaceGuid, ssidPtr, bssType, securityEnabled, IntPtr.Zero, out bssListPtr));

				try
				{
					return ConvertBssListPtr(bssListPtr);
				}
				finally
				{
					WlanapiBase.WlanFreeMemory(bssListPtr);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(ssidPtr);
			}
		}

		/// <summary>
		/// Connects to a network defined by a connection parameters structure.
		/// 连接到由连接参数结构定义的网络。
		/// </summary>
		/// <param name="connectionParams">The connection paramters.</param>
		protected void Connect(WlanConnectionParameters connectionParams)
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanConnect(_manager._clientHandle, _info.InterfaceGuid, ref connectionParams, IntPtr.Zero));
		}

		/// <summary>
		/// Requests a connection (association) to the specified wireless network.
		/// 请求与指定无线网络的连接（关联）。
		/// </summary>
		/// <remarks>
		/// The method returns immediately. Progress is reported through the <see cref="WlanNotification"/> event.
		/// </remarks>
		public void Connect(WlanConnectionMode connectionMode, Dot11BssType bssType, string profile)
		{
            WlanConnectionParameters connectionParams = new WlanConnectionParameters
            {
                WlanConnectionMode = connectionMode,
                Profile = profile,
                Dot11BssType = bssType,
                Flags = 0
            };
            Connect(connectionParams);
		}

		/// <summary>
		/// Connects to the specified wireless network.
		/// 连接到指定的无线网络。
		/// </summary>
		/// <remarks>
		/// The method returns immediately. Progress is reported through the <see cref="WlanNotification"/> event.
		/// </remarks>
		public void Connect(WlanConnectionMode connectionMode, Dot11BssType bssType, Dot11Ssid ssid, WlanConnectionFlags flags)
		{
            WlanConnectionParameters connectionParams = new WlanConnectionParameters
            {
                WlanConnectionMode = connectionMode,
                Dot11SsidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ssid))
            };
            Marshal.StructureToPtr(ssid, connectionParams.Dot11SsidPtr, false);
			connectionParams.Dot11BssType = bssType;
			connectionParams.Flags = flags;

			Connect(connectionParams);

			Marshal.DestroyStructure(connectionParams.Dot11SsidPtr, ssid.GetType());
			Marshal.FreeHGlobal(connectionParams.Dot11SsidPtr);
		}

		/// <summary>
		/// Connects (associates) to the specified wireless network, returning either on a success to connect
		/// or a failure.
		/// 连接（关联）到指定的无线网络，连接成功后返回或故障。
		/// </summary>
		/// <param name="connectionMode"></param>
		/// <param name="bssType"></param>
		/// <param name="profile"></param>
		/// <param name="connectTimeout"></param>
		/// <returns></returns>
		public bool ConnectSynchronously(WlanConnectionMode connectionMode, Dot11BssType bssType, string profile, int connectTimeout)
		{
			// NOTE: This can cause side effects, other places in the application might not get events properly.
			// 注意：这可能会导致副作用，应用程序中的其他位置可能无法正确获取事件
			_queueEvents = true; 
			try
			{
				Connect(connectionMode, bssType, profile);
				while (_queueEvents && _eventQueueFilled.WaitOne(connectTimeout, true))
				{
					lock (_eventQueue)
					{
						while (_eventQueue.Count != 0)
						{
							object e = _eventQueue.Dequeue();
                            if (e is WlanConnectionNotificationEventData wlanConnectionData)
                            {
								// Check if the conditions are good to indicate either success or failure.
								// 检查条件是否良好，以指示成功或失败。
								if (wlanConnectionData.NotifyData.NotificationSource == WlanNotificationSource.WLAN_NOTIFICATION_SOURCE_MSM)
                                {
                                    switch ((WlanNotificationCodeMsm)wlanConnectionData.NotifyData.NotificationCode)
                                    {
                                        case WlanNotificationCodeMsm.Connected:
                                            if (wlanConnectionData.ConnNotifyData.ProfileName == profile)
                                                return true;
                                            break;
                                    }
                                }
                                break;
                            }
                        }
					}
				}
			}
			finally
			{
				_queueEvents = false;
				_eventQueue.Clear();
			}
			// timeout expired and no "connection complete"
			// 超时已过期，没有“连接完成”
			return false; 
		}

		/// <summary>
		/// 断开当前的连接
		/// Disconnect the current connection
		/// </summary>
		public void Disconnect()
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanDisconnect(_manager._clientHandle, _info.InterfaceGuid, IntPtr.Zero));
		}

		/// <summary>
		/// Gets the profile's XML specification.
		/// 获取配置文件的XML规范。
		/// </summary>
		/// <param name="profileName">The name of the profile.</param>
		/// <returns>The XML document.</returns>
		public string GetProfileXml(string profileName)
		{
			IntPtr profileXmlPtr;
			WlanProfileFlags flags;
			WlanAccess access;

			WlanapiBase.ThrowIfError(WlanapiBase.WlanGetProfile(_manager._clientHandle, _info.InterfaceGuid, profileName, IntPtr.Zero, out profileXmlPtr, out flags, out access));

			try
			{
				return Marshal.PtrToStringUni(profileXmlPtr);
			}
			finally
			{
				WlanapiBase.WlanFreeMemory(profileXmlPtr);
			}
		}

		/// <summary>
		/// Gets the information of all profiles on this interface.
		/// 获取此接口上所有配置文件的信息。
		/// </summary>
		/// <returns>The profiles information.</returns>
		public WlanProfileInfo[] GetProfiles()
		{
			IntPtr profileListPtr;
			WlanapiBase.ThrowIfError(WlanapiBase.WlanGetProfileList(_manager._clientHandle, _info.InterfaceGuid, IntPtr.Zero, out profileListPtr));

			try
			{
				WlanProfileInfoListHeader header = (WlanProfileInfoListHeader)Marshal.PtrToStructure(profileListPtr, typeof(WlanProfileInfoListHeader));
				WlanProfileInfo[] profileInfos = new WlanProfileInfo[header.NumberOfItems];
				long profileListIterator = profileListPtr.ToInt64() + Marshal.SizeOf(header);

				for (int i = 0; i < header.NumberOfItems; ++i)
				{
					WlanProfileInfo profileInfo = (WlanProfileInfo)Marshal.PtrToStructure(new IntPtr(profileListIterator), typeof(WlanProfileInfo));
					profileInfos[i] = profileInfo;
					profileListIterator += Marshal.SizeOf(profileInfo);
				}

				return profileInfos;
			}
			finally
			{
				WlanapiBase.WlanFreeMemory(profileListPtr);
			}
		}

		/// <summary>
		/// Sets the profile.
		/// 设置配置文件。
		/// </summary>
		/// <param name="flags">The flags to set on the profile.</param>
		/// <param name="profileXml">The XML representation of the profile. On Windows XP SP 2, special care should be taken to adhere to its limitations.</param>
		/// <param name="overwrite">If a profile by the given name already exists, then specifies whether to overwrite it (if <c>true</c>) or return an error (if <c>false</c>).</param>
		/// <returns>The resulting code indicating a success or the reason why the profile wasn't valid.</returns>
		public WlanReasonCode SetProfile(WlanProfileFlags flags, string profileXml, bool overwrite)
		{
			WlanReasonCode reasonCode;

			WlanapiBase.ThrowIfError(WlanapiBase.WlanSetProfile(_manager._clientHandle, _info.InterfaceGuid, flags, profileXml, null, overwrite, IntPtr.Zero, out reasonCode));

			return reasonCode;
		}

		/// <summary>
		/// Deletes a profile.
		/// 删除配置文件。
		/// </summary>
		/// <param name="profileName">
		/// The name of the profile to be deleted. Profile names are case-sensitive.
		/// On Windows XP SP2, the supplied name must match the profile name derived automatically from the SSID of the network. For an infrastructure network profile, the SSID must be supplied for the profile name. For an ad hoc network profile, the supplied name must be the SSID of the ad hoc network followed by <c>-adhoc</c>.
		/// </param>
		public void DeleteProfile(string profileName)
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanDeleteProfile(_manager._clientHandle, _info.InterfaceGuid, profileName, IntPtr.Zero));
		}

		// TODO:弄清楚作用
		public void SetEAP(string profileName, string userXML)
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanSetProfileEapXmlUserData(_manager._clientHandle, _info.InterfaceGuid, profileName, SetEapUserDataMode.None, userXML, IntPtr.Zero));
		}

		/// <summary>
		/// 当有Wlan连接时
		/// </summary>
		/// <param name="notifyData"></param>
		/// <param name="connNotifyData"></param>
		internal void OnWlanConnection(WlanNotificationData notifyData, WlanConnectionNotificationData connNotifyData)
		{
            WlanConnectionNotification?.Invoke(notifyData, connNotifyData);

            if (_queueEvents)
			{
                WlanConnectionNotificationEventData queuedEvent = new WlanConnectionNotificationEventData
                {
                    NotifyData = notifyData,
                    ConnNotifyData = connNotifyData
                };
                EnqueueEvent(queuedEvent);
			}
		}

		internal void OnWlanReason(WlanNotificationData notifyData, WlanReasonCode reasonCode)
		{
            WlanReasonNotification?.Invoke(notifyData, reasonCode);

            if (_queueEvents)
			{
                WlanReasonNotificationData queuedEvent = new WlanReasonNotificationData
                {
                    NotifyData = notifyData,
                    ReasonCode = reasonCode
                };
                EnqueueEvent(queuedEvent);
			}
		}

		/// <summary>
		/// 当有Wlan的通知
		/// </summary>
		/// <param name="notifyData"></param>
		internal void OnWlanNotification(WlanNotificationData notifyData)
		{
            WlanNotification?.Invoke(notifyData);
        }

		/// <summary>
		/// Enqueues a notification event to be processed serially.
		/// 将要串行处理的通知事件排队。
		/// </summary>
		private void EnqueueEvent(object queuedEvent)
		{
			lock (_eventQueue)
				_eventQueue.Enqueue(queuedEvent);

			_eventQueueFilled.Set();
		}

		/// <summary>
		/// Gets a parameter of the interface whose data type is <see cref="int"/>.
		/// 获取其数据类型的接口的参数
		/// 
		/// Possible Win32 errors:
		/// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions to perform the requested operation.
		/// ERROR_INVALID PARAMETER: hClientHandle is NULL or invalid, pInterfaceGuid is NULL, pReserved is not NULL, ppData is NULL, or pdwDataSize is NULL.
		/// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
		/// ERROR_INVALID_STATE: OpCode is set to wlan_intf_opcode_current_connection and the client is not currently connected to a network.
		/// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
		/// RPC_STATUS: Various error codes.
		/// </summary>
		/// <param name="opCode">The opcode of the parameter.</param>
		/// <returns>The integer value.</returns>
		private int GetInterfaceInt(WlanIntfOpcode opCode)
		{
			WlanapiBase.ThrowIfError(WlanapiBase.WlanQueryInterface(_manager._clientHandle, _info.InterfaceGuid, opCode, IntPtr.Zero, out int valueSize, out IntPtr valuePtr, out WlanOpcodeValueType opcodeValueType));

            try
			{
				return Marshal.ReadInt32(valuePtr);
			}
			finally
			{
				WlanapiBase.WlanFreeMemory(valuePtr);
			}
		}

		/// <summary>
		/// Sets a parameter of the interface whose data type is <see cref="int"/>.
		/// 设置其数据类型的接口的参数
		/// 
		/// Possible Win32 errors:
		/// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions to perform the requested operation.
		/// ERROR_GEN_FAILURE: The parameter specified by OpCode is not supported by the driver or NIC.
		/// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
		/// ERROR_INVALID_PARAMETER: One parameter is likely NULL
		/// RPC_STATUS: Various return codes to indicate errors occurred when connecting.
		/// </summary>
		/// <param name="opCode">The opcode of the parameter.</param>
		/// <param name="value">The value to set.</param>
		private void SetInterfaceInt(WlanIntfOpcode opCode, int value)
		{
			IntPtr valuePtr = Marshal.AllocHGlobal(sizeof(int));
			Marshal.WriteInt32(valuePtr, value);

			try
			{
				WlanapiBase.ThrowIfError(WlanapiBase.WlanSetInterface(_manager._clientHandle, _info.InterfaceGuid, opCode, sizeof(int), valuePtr, IntPtr.Zero));
			}
			finally
			{
				Marshal.FreeHGlobal(valuePtr);
			}
		}
		#endregion
	}
}
