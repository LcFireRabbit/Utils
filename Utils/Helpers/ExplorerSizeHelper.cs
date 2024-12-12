using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Helpers
{
    public class ExplorerSizeHelper
    {
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileLength"></param>
        /// <returns></returns>
        public static bool GetFileSize(string filePath, out long fileLength)
        {
            fileLength = -1;
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileLength = fileInfo.Length;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取文件夹及子目录文件大小
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="directoryLength"></param>
        /// <returns></returns>
        public static bool GetDirectorySize(string directoryPath, out long directoryLength)
        {
            directoryLength= -1;
            if (Directory.Exists(directoryPath))
            {
                //一级目录
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                foreach (var item in directoryInfo.GetFiles())
                {
                    directoryLength += item.Length;
                }
                //子目录
                foreach (var item in directoryInfo.GetDirectories())
                {
                    if (GetDirectorySize(item.FullName,out long dirLength))
                    {
                        directoryLength += dirLength;
                    }
                }

                return true;
            }
            return false;
        }
    }
}
