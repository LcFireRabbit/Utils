using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
	internal struct WlanConnectionNotificationEventData
	{
		public WlanNotificationData NotifyData;
		public WlanConnectionNotificationData ConnNotifyData;
	}

	internal struct WlanReasonNotificationData
	{
		public WlanNotificationData NotifyData;
		public WlanReasonCode ReasonCode;
	}
}
