using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public enum ExecuteResult
    {
        Success=0,
        Failed=1,
        ReStart=2,
    }

    public class Common
    {
        /// <summary>
        /// 判断Windows系统是否为旧版本
        /// </summary>
        /// <returns></returns>
        public static bool IsOldOsVersion()
        {
            OperatingSystem os = Environment.OSVersion;
            return os.Platform != PlatformID.Win32NT || os.Version.Major < 6;
        }
    }
}
