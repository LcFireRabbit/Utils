using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Theme
    {
        /// <summary>
        /// 获取当前操作系统 是浅色还是暗色
        /// </summary>
        /// <returns></returns>
        public static bool IsLight()
        {
            bool isLightMode = true;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                if (v != null && v.ToString() == "0")
                    isLightMode = false;
            }
            catch { }
            return isLightMode;
        }
    }
}
