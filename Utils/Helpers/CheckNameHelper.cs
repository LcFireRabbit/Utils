using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Helpers
{
    public static class CheckNameHelper
    {
        /// <summary>
        /// 文件或文件夹名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckFileOrDirectory(string name)
        {
            bool isValid = true;
            string[] strList = new string[] { " ", "/", "\"", @"\", @"\/", ":", "*", "?", "<", ">", "|", "\r\n" };

            if (string.IsNullOrEmpty(name))
            {
                isValid = false;
            }
            else
            {
                foreach (string errStr in strList)
                {
                    if (name.Contains(errStr))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            return isValid;
        }
    }
}
