using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public partial class Shell32Base
    {
        /// <summary>         
        /// 文件信息标识枚举类,所有枚举定义值前省略SHGFI投标，比如Icon 完整名称应为SHGFI_ICON         
        /// </summary>         
        [Flags]
        public enum SHFileInfoFlags : uint
        {
            /// <summary>             
            /// 允许有叠加图案的文件图标，该标识必须和Icon同时使用             
            /// </summary>             
            AddOveylays = 0x20,         // SHGFI_AddOverlays = 0x000000020
            /// <summary>             
            /// 只获取由参数FileAttribute指定的文件信息，并将其写入SHFileInfo结构的dwAttributes属性，如果不指定该标识，将同时获取所有文件信息。该标志不能和Icon标识同时使用             
            /// </summary>             
            Attr_Specified = 0x20000,   //  SHGFI_SpecifiedAttributes = 0x000020000
            /// <summary>             
            /// 将获取的文件属性复制到SHFileInfo结构的dwAttributes属性中             
            /// </summary>             
            Attributes = 0x800,     // SHGFI_Attributes = 0x000000800
            /// <summary>             
            /// 获取文件的显示名称（长文件名称），将其复制到SHFileInfo结构的dwAttributes属性中             
            /// </summary>             
            DisplayName = 0x200,    // SHGFI_DisplayName = 0x000000200
            /// <summary>            
            /// 如果文件是可执行文件，将检索其信息，并将信息作为返回值返回              
            /// </summary>             
            ExeType = 0x2000,       // SHGFI_EXEType = 0x000002000
            /// <summary>             
            /// 获得图标和索引，将图标句柄返回到SHFileInfo结构的hIcon属性中，索引返回到iIcon属性中             
            /// </summary>             
            Icon = 0x100,           // SHGFI_Icon = 0x000000100
            /// <summary>             
            /// 检索包含图标的文件，并将文件名，图标句柄，图标索引号，放回到SHFileInfo结构中             
            /// </summary>             
            IconLocation = 0x1000,  // SHGFI_IconLocation = 0x000001000
            /// <summary>             
            /// 获得大图标，该标识必须和Icon标识同时使用             
            /// </summary>             
            LargeIcon = 0x0,        // SHGFI_LargeIcon = 0x000000000
            /// <summary>             
            /// 获取链接覆盖文件图标，该标识必须和Icon标识同时使用。             
            /// </summary>             
            LinkOverlay = 0x8000,   // SHGFI_LinkOverlay = 0x000008000
            /// <summary>             
            /// 获取文件打开时的图标，该标识必须和Icon或SysIconIndex同时使用             
            /// </summary>             
            OpenIcon = 0x2,         //  SHGFI_OpenIcon = 0x000000002
            /// <summary>             
            /// 获取链接覆盖文件图标索引，该标识必须和Icon标识同时使用。             
            /// </summary>             
            OverlayIndex = 0x40,    // SHGFI_OverlayIndex = 0x000000040
            /// <summary>             
            /// 指示传入的路径是一个ITEMIDLIST结构的文件地址而不是一个路径名。             
            /// </summary>             
            Pidl = 0x8,             // SHGFI_PIDL = 0x000000008
            /// <summary>             
            /// 获取系统的高亮显示图标，该标识必须和Icon标识同时使用。             
            /// </summary>             
            Selected = 0x10000,     // SHGFI_SelectedState = 0x000010000
            /// <summary>             
            /// 获取 Shell-sized icon ，该标志必须和Icon标识同时使用。             
            /// </summary>             
            ShellIconSize = 0x4,    // SHGFI_ShellIconSize = 0x000000004
            /// <summary>             
            /// 获得小图标，该标识必须和Icon或SysIconIndex同时使用。             
            /// </summary>             
            SmallIcon = 0x1,       // SHGFI_SmallIcon = 0x000000001
            /// <summary>             
            /// 获取系统图像列表图标索引，返回系统图像列表句柄             
            /// </summary>             
            SysIconIndex = 0x4000,  // SHGFI_SysIconIndex = 0x000004000
            /// <summary>             
            /// 获得文件类型，类型字符串被写入SHFileInfo结构的szTypeName属性中             
            /// </summary>             
            TypeName = 0x400,       // SHGFI_TypeName = 0x000000400
            /// <summary>             
            /// 指示如果由pszPath指定的路径不存在，SHGetFileInfo方法变不会试图去操作文件。指示返回与文件类型相关的信息。该标识不能和Attributes、ExeType和Pidl同时使用             
            /// </summary>             
            UseFileAttributes = 0x10    // SHGFI_UserFileAttributes = 0x000000010,
        }

        /// <summary>         
        /// 文件属性枚举         
        /// </summary>         
        [Flags]
        public enum FileAttribute
        {
            ReadOnly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,     //路径信息             
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,        //普通文件信息             
            Temporary = 0x00000100,
            Sparse_File = 0x00000200,
            Reparse_Point = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            Not_Content_Indexed = 0x00002000,
            Encrypted = 0x00004000
        }

        /// <summary>        
        /// 定义返回的文件信息结构         
        /// </summary>         
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFileInfo
        {
            /// <summary>             
            /// 文件的图标句柄             
            /// </summary>             
            public IntPtr hIcon;
            /// <summary>             
            /// 图标的系统索引号             
            /// </summary>             
            public IntPtr iIcon;
            /// <summary>             
            /// 文件的属性值,由FileAttribute指定的属性。             
            /// </summary>             
            public uint dwAttributes;
            /// <summary>            
            /// 文件的显示名,如果是64位系统，您可能需要制定SizeConst=258而非260才能够显示完整的 TypeName             
            /// </summary>             
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            /// <summary>             
            /// 文件的类型名             
            /// </summary>             
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}
