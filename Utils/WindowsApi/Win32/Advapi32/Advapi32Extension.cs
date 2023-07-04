using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Advapi32Extension : Advapi32Base
    {
        /// <summary>
        /// 授予权限
        /// 参数取值为文件PrivilegeAPI中的类PrivilegeConstants中的字段
        /// </summary>
        /// <param name="privilegeName">PrivilegeConstants类中的字段</param>
        /// <returns></returns>
        public static bool GrantPrivilege(string privilegeName)
        {
            try
            {
                LUID locallyUniqueIdentifier = new LUID();
                if (LookupPrivilegeValue(null, privilegeName, ref locallyUniqueIdentifier))
                {
                    TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();
                    tokenPrivileges.PrivilegeCount = 1;

                    LUID_AND_ATTRIBUTES luidAndAtt = new LUID_AND_ATTRIBUTES();
                    // luidAndAtt.Attributes should be SE_PRIVILEGE_ENABLED to enable privilege
                    luidAndAtt.Attributes = SE_PRIVILEGE_ENABLED;
                    luidAndAtt.Luid = locallyUniqueIdentifier;
                    tokenPrivileges.Privilege = luidAndAtt;

                    IntPtr tokenHandle = IntPtr.Zero;
                    try
                    {
                        if (OpenProcessToken(Kernel32Base.GetCurrentProcess(),
                            TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandle))
                        {
                            if (AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 1024, IntPtr.Zero, 0))
                            {
                                // 当前用户没有关联该权限
                                // 需要在windows系统（本地安全策略——本地策略——用户权限分配）中设置为该权限添加当前用户
                                if (Marshal.GetLastWin32Error() != ERROR_NOT_ALL_ASSIGNED)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (tokenHandle != IntPtr.Zero)
                        {
                            Kernel32Base.CloseHandle(tokenHandle);
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 撤销权限
        /// 参数取值为文件PrivilegeAPI中的类PrivilegeConstants中的字段
        /// </summary>
        /// <param name="privilegeName">PrivilegeConstants类中的字段</param>
        /// <returns></returns>
        public static bool RevokePrivilege(string privilegeName)
        {
            try
            {
                LUID locallyUniqueIdentifier = new LUID();

                if (LookupPrivilegeValue(null, privilegeName, ref locallyUniqueIdentifier))
                {
                    TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();
                    tokenPrivileges.PrivilegeCount = 1;

                    LUID_AND_ATTRIBUTES luidAndAtt = new LUID_AND_ATTRIBUTES();
                    // luidAndAtt.Attributes should be none (not set) to disable privilege
                    luidAndAtt.Luid = locallyUniqueIdentifier;
                    tokenPrivileges.Privilege = luidAndAtt;

                    IntPtr tokenHandle = IntPtr.Zero;
                    try
                    {
                        if (OpenProcessToken(Kernel32Base.GetCurrentProcess(),
                            TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandle))
                        {
                            if (AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 1024, IntPtr.Zero, 0))
                            {
                                // 当前用户没有关联该权限
                                // 需要在windows系统（本地安全策略——本地策略——用户权限分配）中设置为该权限添加当前用户
                                if (Marshal.GetLastWin32Error() != ERROR_NOT_ALL_ASSIGNED)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (tokenHandle != IntPtr.Zero)
                        {
                            Kernel32Base.CloseHandle(tokenHandle);
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
