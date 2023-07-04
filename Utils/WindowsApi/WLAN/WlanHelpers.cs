using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    internal class WlanHelpers
    {
		internal static WlanConnectionNotificationData? ParseWlanConnectionNotification(ref WlanNotificationData notifyData)
		{
			int expectedSize = Marshal.SizeOf(typeof(WlanConnectionNotificationData));
			if (notifyData.DataSize < expectedSize)
				return null;

			WlanConnectionNotificationData connNotifyData = (WlanConnectionNotificationData)Marshal.PtrToStructure(notifyData.DataPtr, typeof(WlanConnectionNotificationData));

			if (connNotifyData.WlanReasonCode == WlanReasonCode.Success)
			{
				long profileXmlPtrValue = notifyData.DataPtr.ToInt64() + Marshal.OffsetOf(typeof(WlanConnectionNotificationData), "profileXml").ToInt64();
				connNotifyData.ProfileXml = Marshal.PtrToStringUni(new IntPtr(profileXmlPtrValue));
			}

			return connNotifyData;
		}

		/// <summary>
		/// Gets a string that describes a specified reason code. NOTE: Not used!
		/// 获取描述指定原因代码的字符串。注意：未使用！
		/// </summary>
		/// <param name="reasonCode">The reason code.</param>
		/// <returns>The string.</returns>
		internal static string GetStringForReasonCode(WlanReasonCode reasonCode)
		{
			// the 1024 size here is arbitrary; the WlanReasonCodeToString docs fail to specify a recommended size
			// 这里的1024大小是任意的；WlanReasonCodeToString文档无法指定建议的大小
			StringBuilder sb = new StringBuilder(1024); 

			if (WlanapiBase.WlanReasonCodeToString(reasonCode, sb.Capacity, sb, IntPtr.Zero) != 0)
			{
				// Not sure if we get junk in the stringbuilder buffer from WlanReasonCodeToString, clearing it to be sure. 
				// 不确定是否从WlanReasonCodeToString在stringbuilder缓冲区中得到垃圾，请清除它以确定。
				sb.Clear();
				sb.Append("Failed to retrieve reason code, probably too small buffer.");
			}

			return sb.ToString();
		}
	}
}
