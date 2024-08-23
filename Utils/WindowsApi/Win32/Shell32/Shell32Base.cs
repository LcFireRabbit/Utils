using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public partial class Shell32Base
    {
        private const string Shell32 = "shell32.dll";

        /// <summary>         
        /// 引用shell32文件的SHGetFileInfo API方法         
        /// </summary>         
        /// <param name="pszPath">指定的文件名,如果为""则返回文件夹的</param>         
        /// <param name="dwFileAttributes">文件属性</param>         
        /// <param name="sfi">返回获得的文件信息,是一个记录类型</param>         
        /// <param name="cbFileInfo">文件的类型名</param>        
        /// <param name="uFlags">文件信息标识</param>         
        /// <returns>-1失败</returns>         
        [DllImport(Shell32, EntryPoint = "SHGetFileInfo", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SHGetFileInfo(string pszPath, FileAttribute dwFileAttributes, ref SHFileInfo sfi, uint cbFileInfo, SHFileInfoFlags uFlags);

        /// <summary>   
        /// 返回系统设置的图标   
        /// </summary>   
        /// <param name="lpszFile">文件名,指定从exe文件或dll文件引入icon</param>   
        /// <param name="nIconIndex">文件的图表中的第几个,指定icon的索引如果为0则从指定的文件中引入第1个icon</param>   
        /// <param name="phiconLarge">返回的大图标的指针,大图标句柄如果为null则为没有大图标</param>   
        /// <param name="phiconSmall">返回的小图标的指针,小图标句柄如果为null则为没有小图标</param>   
        /// <param name="nIcons">ico个数,找几个图标</param>   
        /// <returns></returns
        [DllImport(Shell32)]
        public static extern uint ExtractIconEx(string lpszFile, int nIconIndex, int[] phiconLarge, int[] phiconSmall, uint nIcons);

        
    }
}
