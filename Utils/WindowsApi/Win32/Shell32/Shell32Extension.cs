using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Shell32Extension : Shell32Base
    {
        /// <summary>         
        /// 当读取文件图标失败的默认图标索引号         
        /// </summary>         
        public static readonly long ErrorFileIndex = -2;
        /// <summary>         
        /// 当读取文件夹图标失败的默认图标索引号         
        /// </summary>         
        public static readonly long ErrorFolderIndex = -4;
        /// <summary>         
        /// 当读取磁盘驱动器图标失败的默认图标索引号         
        /// </summary>         
        public static readonly long ErrorDriverIndex = -8;
        /// <summary>         
        /// 当读取可执行文件图标失败的默认图标索引号         
        /// </summary>         
        public static readonly long ErrorApplicationIndex = -16;

        /// <summary>
        /// 获取文件类型的关联图标
        /// </summary>
        /// <param name="fileName">文件类型的扩展名或文件的绝对路径</param>
        /// <param name="isSmallIcon">是否返回小图标</param>
        /// <returns>返回一个Icon类型的文件图标对象</returns>
        public static Icon GetFileIcon(string fileName, bool isSmallIcon)
        {
            long imageIndex;
            return GetFileIcon(fileName, isSmallIcon, out imageIndex);
        }

        /// <summary>
        /// 获取系统文件图标
        /// </summary>
        /// <param name="fileName">文件类型的扩展名或文件的绝对路径,如果是一个exe可执行文件，请提供完整的文件名（包含路径信息）</param>
        /// <param name="isSmallIcon">是否返回小图标</param>
        /// <param name="imageIndex">输出与返回图标对应的系统图标索引号</param>
        /// <returns>返回一个Icon类型的文件图标对象</returns>
        public static Icon GetFileIcon(string fileName, bool isSmallIcon, out long imageIndex)
        {
            imageIndex = ErrorFileIndex;
            if (String.IsNullOrEmpty(fileName))
                return null;

            SHFileInfo shfi = new SHFileInfo();
            SHFileInfoFlags uFlags = SHFileInfoFlags.Icon | SHFileInfoFlags.ShellIconSize;
            if (isSmallIcon)
                uFlags |= SHFileInfoFlags.SmallIcon;
            else
                uFlags |= SHFileInfoFlags.LargeIcon;
            FileInfo fi = new FileInfo(fileName);
            if (fi.Name.ToUpper().EndsWith(".EXE"))
                uFlags |= SHFileInfoFlags.ExeType;
            else
                uFlags |= SHFileInfoFlags.UseFileAttributes;

            int iTotal = (int)SHGetFileInfo(fileName, FileAttribute.Normal, ref shfi, (uint)Marshal.SizeOf(shfi), uFlags);
            //或int iTotal = (int)SHGetFileInfo(fileName, 0, ref shfi, (uint)Marshal.SizeOf(shfi), uFlags);
            Icon icon = null;
            if (iTotal > 0)
            {
                icon = Icon.FromHandle(shfi.hIcon).Clone() as Icon;
                imageIndex = shfi.iIcon.ToInt64();
            }
            User32Base.DestroyIcon(shfi.hIcon); //释放资源
            return icon;
        }

        /// <summary>  
        /// 获取系统文件夹默认图标
        /// </summary>  
        /// <param name="isSmallIcon">是否返回小图标</param>
        /// <returns>返回一个Icon类型的文件夹图标对象</returns>
        public static Icon GetFolderIcon(bool isSmallIcon)
        {
            long imageIndex;
            return GetFolderIcon(isSmallIcon, out imageIndex);
        }

        /// <summary>  
        /// 获取系统文件夹默认图标
        /// </summary>  
        /// <param name="isSmallIcon">是否返回小图标</param>
        /// <param name="imageIndex">输出与返回图标对应的系统图标索引号</param>
        /// <returns>返回一个Icon类型的文件夹图标对象</returns>
        public static Icon GetFolderIcon(bool isSmallIcon, out long imageIndex)
        {
            return GetFolderIcon(Environment.SystemDirectory, isSmallIcon, out imageIndex);
        }

        /// <summary>  
        /// 获取系统文件夹默认图标
        /// </summary>  
        /// <param name="folderName">文件夹名称,如果想获取自定义文件夹图标，请指定完整的文件夹名称（如 F:\test)</param>
        /// <param name="isSmallIcon">是否返回小图标</param>
        /// <param name="imageIndex">输出与返回图标对应的系统图标索引号</param>
        /// <returns>返回一个Icon类型的文件夹图标对象</returns>
        public static Icon GetFolderIcon(string folderName, bool isSmallIcon, out long imageIndex)
        {
            imageIndex = ErrorFolderIndex;
            if (String.IsNullOrEmpty(folderName))
                return null;

            SHFileInfo shfi = new SHFileInfo();
            SHFileInfoFlags uFlags = SHFileInfoFlags.Icon | SHFileInfoFlags.ShellIconSize | SHFileInfoFlags.UseFileAttributes;
            if (isSmallIcon)
                uFlags |= SHFileInfoFlags.SmallIcon;
            else
                uFlags |= SHFileInfoFlags.LargeIcon;

            int iTotal = (int)SHGetFileInfo(folderName, FileAttribute.Directory, ref shfi, (uint)Marshal.SizeOf(shfi), uFlags);
            //或int iTotal = (int)SHGetFileInfo("", 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHFileInfoFlags.Icon | SHFileInfoFlags.SmallIcon);
            Icon icon = null;
            if (iTotal > 0)
            {
                icon = Icon.FromHandle(shfi.hIcon).Clone() as Icon;
                imageIndex = shfi.iIcon.ToInt64();
            }
            User32Base.DestroyIcon(shfi.hIcon); //释放资源
            return icon;
        }

        /// <summary>         
        /// 获取磁盘驱动器图标
        /// </summary>         
        /// <param name="driverMark">有效的磁盘标号，如C、D、I等等，不区分大小写</param>         
        /// <param name="isSmallIcon">标识是获取小图标还是获取大图标</param>         
        /// <param name="imageIndex">输出与返回图标对应的系统图标索引号</param>         
        /// <returns>返回一个Icon类型的磁盘驱动器图标对象</returns>         
        public static Icon GetDriverIcon(char driverMark, bool isSmallIcon)
        {
            long imageIndex;
            return GetDriverIcon(driverMark, isSmallIcon, out imageIndex);
        }

        /// <summary>         
        /// 获取磁盘驱动器图标
        /// </summary>         
        /// <param name="driverMark">有效的磁盘标号，如C、D、I等等，不区分大小写</param>         
        /// <param name="isSmallIcon">标识是获取小图标还是获取大图标</param>         
        /// <param name="imageIndex">输出与返回图标对应的系统图标索引号</param>         
        /// <returns>返回一个Icon类型的磁盘驱动器图标对象</returns>         
        public static Icon GetDriverIcon(char driverMark, bool isSmallIcon, out long imageIndex)
        {
            imageIndex = ErrorDriverIndex;
            //非有效盘符，返回封装的磁盘图标             
            if (driverMark < 'a' && driverMark > 'z' && driverMark < 'A' && driverMark > 'Z')
            {
                return null;
            }
            string driverName = driverMark.ToString().ToUpper() + ":\\";

            SHFileInfo shfi = new SHFileInfo();
            SHFileInfoFlags uFlags = SHFileInfoFlags.Icon | SHFileInfoFlags.ShellIconSize | SHFileInfoFlags.UseFileAttributes;
            if (isSmallIcon)
                uFlags |= SHFileInfoFlags.SmallIcon;
            else
                uFlags |= SHFileInfoFlags.LargeIcon;
            int iTotal = (int)SHGetFileInfo(driverName, FileAttribute.Normal, ref shfi, (uint)Marshal.SizeOf(shfi), uFlags);
            //int iTotal = (int)SHGetFileInfo(driverName, 0, ref shfi, (uint)Marshal.SizeOf(shfi), uFlags);
            Icon icon = null;
            if (iTotal > 0)
            {
                icon = Icon.FromHandle(shfi.hIcon).Clone() as Icon;
                imageIndex = shfi.iIcon.ToInt64();
            }
            User32Base.DestroyIcon(shfi.hIcon); //释放资源
            return icon;
        }
    }
}
