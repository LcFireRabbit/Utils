using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class AttendeeInfo
    {
        public string RemoteName { get; set; }
        public string PeerIP { get; set; }
        public int ID { get; set; }
        public bool IsControl { get; set; }
    }
}
