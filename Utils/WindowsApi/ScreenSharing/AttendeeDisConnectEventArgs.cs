using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class AttendeeDisConnectEventArgs
    {
        public int ID { get; set; }

        public AttendeeDisConnectEventArgs(int id)
        {
            ID = id;
        }
    }
}
