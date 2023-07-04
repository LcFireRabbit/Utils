using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    /// <summary>
    /// System WLAN Manager
    /// </summary>
    public class WlanManager
    {
        #region Fields
        /// <summary>
        /// 客户端在此会话中使用的句柄
        /// </summary>
        internal IntPtr _clientHandle;

        /// <summary>
        /// 将在此会话中使用的 WLAN API 版本
        /// </summary>
        internal uint _negotiatedVersion;

        /// <summary>
        /// 通知事件回调
        /// </summary>
        internal WlanapiBase.WlanNotificationCallbackDelegate _wlanNotificationCallback;

        /// <summary>
        /// 无线网卡列表
        /// </summary>
        private Dictionary<Guid, WlanInterface> _ifaces = new Dictionary<Guid, WlanInterface>();
        #endregion

        #region Properties
        /// <summary>
        /// 没有WIFI
        /// </summary>
        public bool NoWifiAvailable { get; set; }

        /// <summary>
		/// Gets the WLAN interfaces.
        /// 获取WLAN接口
		/// 
		/// Possible Win32 exceptions:
        /// 可能的Win32异常：
		/// 
		/// ERROR_INVALID_PARAMETER: A parameter is incorrect. This error is returned if the hClientHandle or ppInterfaceList parameter is NULL. This error is returned if the pReserved is not NULL. This error is also returned if the hClientHandle parameter is not valid.
		/// ERROR_INVALID_HANDLE: The handle hClientHandle was not found in the handle table.
		/// RPC_STATUS: Various error codes.
		/// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request and allocate memory for the query results.
		/// </summary>
		/// <value>The WLAN interfaces.</value>
		public WlanInterface[] Interfaces
        {
            get
            {
                if (NoWifiAvailable)
                    return null;

                WlanapiBase.ThrowIfError(WlanapiBase.WlanEnumInterfaces(_clientHandle, IntPtr.Zero, out IntPtr ifaceList));

                try
                {
                    WlanInterfaceInfoListHeader header = (WlanInterfaceInfoListHeader)Marshal.PtrToStructure(ifaceList, typeof(WlanInterfaceInfoListHeader));

                    Int64 listIterator = ifaceList.ToInt64() + Marshal.SizeOf(header);
                    WlanInterface[] interfaces = new WlanInterface[header.NumberOfItems];
                    List<Guid> currentIfaceGuids = new List<Guid>();

                    for (int i = 0; i < header.NumberOfItems; ++i)
                    {
                        WlanInterfaceInfo info = (WlanInterfaceInfo)Marshal.PtrToStructure(new IntPtr(listIterator), typeof(WlanInterfaceInfo));

                        listIterator += Marshal.SizeOf(info);
                        currentIfaceGuids.Add(info.InterfaceGuid);

                        WlanInterface wlanIface;
                        if (_ifaces.ContainsKey(info.InterfaceGuid))
                            wlanIface = _ifaces[info.InterfaceGuid];
                        else
                            wlanIface = new WlanInterface(this, info);

                        interfaces[i] = wlanIface;
                        _ifaces[info.InterfaceGuid] = wlanIface;
                    }

                    // Remove stale interfaces
                    Queue<Guid> deadIfacesGuids = new Queue<Guid>();
                    foreach (Guid ifaceGuid in _ifaces.Keys)
                    {
                        if (!currentIfaceGuids.Contains(ifaceGuid))
                            deadIfacesGuids.Enqueue(ifaceGuid);
                    }

                    while (deadIfacesGuids.Count != 0)
                    {
                        Guid deadIfaceGuid = deadIfacesGuids.Dequeue();
                        _ifaces.Remove(deadIfaceGuid);
                    }

                    return interfaces;
                }
                finally
                {
                    WlanapiBase.WlanFreeMemory(ifaceList);
                }
            }
        }
        #endregion

        #region Constructor
        public WlanManager()
        {
            int errorCode = 0;
            //限制条件
            OperatingSystem osVersion = Environment.OSVersion;
            bool isWinXP = osVersion.Platform == PlatformID.Win32NT && osVersion.Version.Major == 5 && osVersion.Version.Minor != 0;
            // wlanapi not supported in sp1 (or sp2 without hotfix)
            if (isWinXP && osVersion.ServicePack == "Service Pack 1")
            {
                errorCode = Win32ErrorCodes.ERROR_SERVICE_NOT_ACTIVE;
            }
            else
            {
                //连接WLAN服务
                try
                {
                    errorCode = WlanapiBase.WlanOpenHandle(WlanapiBase.WLAN_CLIENT_VERSION_XP_SP2, IntPtr.Zero, out _negotiatedVersion, out _clientHandle);
                }
                catch
                {
                    errorCode = Win32ErrorCodes.ERROR_SERVICE_NOT_ACTIVE;
                }
            }
            //判断是否开启成功
            if (errorCode != Win32ErrorCodes.ERROR_SUCCESS)
            {
                NoWifiAvailable = true;
                return;
            }
            //注册WLAN的通知
            try
            {
                // Interop callback
                _wlanNotificationCallback = new WlanapiBase.WlanNotificationCallbackDelegate(OnWlanNotification);

                WlanapiBase.ThrowIfError(WlanapiBase.WlanRegisterNotification(_clientHandle,
                    WlanNotificationSource.WLAN_NOTIFICATION_SOURCE_ALL, false, 
                    _wlanNotificationCallback, IntPtr.Zero, IntPtr.Zero, out WlanNotificationSource prevSrc));
            }
            catch
            {
                WlanapiBase.WlanCloseHandle(_clientHandle, IntPtr.Zero);
                throw new Exception();
            }
        }

        ~WlanManager()
        {
            // Free the handle when deconstructing the client. There won't be a handle if its xp sp 2 without wlanapi installed
            // 释放实例
            try
            {
                WlanapiBase.WlanCloseHandle(_clientHandle, IntPtr.Zero);
            }
            catch
            { }
        }
        #endregion

        #region Fuc
        /// <summary>
        /// 当无线通知时
        /// </summary>
        /// <param name="notifyData"></param>
        /// <param name="context"></param>
        private void OnWlanNotification(ref WlanNotificationData notifyData, IntPtr context)
        {
            if (NoWifiAvailable)
                return;

            WlanInterface wlanIface = _ifaces.ContainsKey(notifyData.InterfaceGuid) ? _ifaces[notifyData.InterfaceGuid] : null;

            switch (notifyData.NotificationSource)
            {
                case WlanNotificationSource.WLAN_NOTIFICATION_SOURCE_ACM:
                    switch ((WlanNotificationCodeAcm)notifyData.NotificationCode)
                    {
                        case WlanNotificationCodeAcm.ConnectionStart:
                        case WlanNotificationCodeAcm.ConnectionComplete:
                        case WlanNotificationCodeAcm.ConnectionAttemptFail:
                        case WlanNotificationCodeAcm.Disconnecting:
                        case WlanNotificationCodeAcm.Disconnected:
                            WlanConnectionNotificationData? connNotifyData = WlanHelpers.ParseWlanConnectionNotification(ref notifyData);

                            if (connNotifyData.HasValue && wlanIface != null)
                                wlanIface.OnWlanConnection(notifyData, connNotifyData.Value);

                            break;
                        case WlanNotificationCodeAcm.ScanFail:
                            int expectedSize = Marshal.SizeOf(typeof(int));

                            if (notifyData.DataSize >= expectedSize)
                            {
                                int reasonInt = Marshal.ReadInt32(notifyData.DataPtr);

                                // Want to make sure this doesn't crash if windows sends a reasoncode not defined in the enum.
                                // 希望确保在windows发送枚举中未定义的推理码时不会崩溃。
                                if (Enum.IsDefined(typeof(WlanReasonCode), reasonInt))
                                {
                                    WlanReasonCode reasonCode = (WlanReasonCode)reasonInt;

                                    if (wlanIface != null)
                                        wlanIface.OnWlanReason(notifyData, reasonCode);
                                }
                            }
                            break;
                    }
                    break;
                case WlanNotificationSource.WLAN_NOTIFICATION_SOURCE_MSM:
                    switch ((WlanNotificationCodeMsm)notifyData.NotificationCode)
                    {
                        case WlanNotificationCodeMsm.Associating:
                        case WlanNotificationCodeMsm.Associated:
                        case WlanNotificationCodeMsm.Authenticating:
                        case WlanNotificationCodeMsm.Connected:
                        case WlanNotificationCodeMsm.RoamingStart:
                        case WlanNotificationCodeMsm.RoamingEnd:
                        case WlanNotificationCodeMsm.Disassociating:
                        case WlanNotificationCodeMsm.Disconnected:
                        case WlanNotificationCodeMsm.PeerJoin:
                        case WlanNotificationCodeMsm.PeerLeave:
                        case WlanNotificationCodeMsm.AdapterRemoval:
                            WlanConnectionNotificationData? connNotifyData = WlanHelpers.ParseWlanConnectionNotification(ref notifyData);

                            if (connNotifyData.HasValue && wlanIface != null)
                                wlanIface.OnWlanConnection(notifyData, connNotifyData.Value);

                            break;
                    }
                    break;
            }

            if (wlanIface != null)
                wlanIface.OnWlanNotification(notifyData);
        }
        #endregion
    }
}
