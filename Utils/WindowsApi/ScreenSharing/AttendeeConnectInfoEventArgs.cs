using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class AttendeeConnectInfoEventArgs : AttendeeInfo
    {
        public AttendeeConnectInfoEventArgs(string name, string ip, int id, bool isContol)
        {
            RemoteName = name;
            PeerIP = ip;
            ID = id;
            IsControl = isContol;
        }
    }
}
