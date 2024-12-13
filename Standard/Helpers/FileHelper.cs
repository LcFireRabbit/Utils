using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standard.Helpers
{
    public static class FileHelper
    {
        public static bool CheckFileOrDirectoryNameValidity(string name)
        {
            string[] strList = new string[] { " ", "/", "\"", @"\", @"\/", ":", "*", "?", "<", ">", "|", "\r\n" };

            return !string.IsNullOrEmpty(name) && !strList.Any(p => name.Contains(p));
        }
    }
}
