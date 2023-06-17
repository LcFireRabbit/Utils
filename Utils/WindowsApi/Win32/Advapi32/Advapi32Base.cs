using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Advapi32Base
    {
        private const string Advapi32 = "Advapi32.dll";

        // 如果进程的访问令牌中没有关联某权限，则AdjustTokenPrivileges函数调用将会返回错误码ERROR_NOT_ALL_ASSIGNED（值为1300）
        public const int ERROR_NOT_ALL_ASSIGNED = 1300;

        #region TokenAccess
        internal const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        internal const uint STANDARD_RIGHTS_READ = 0x00020000;
        internal const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const uint TOKEN_DUPLICATE = 0x0002;
        internal const uint TOKEN_IMPERSONATE = 0x0004;
        internal const uint TOKEN_QUERY = 0x0008;
        internal const uint TOKEN_QUERY_SOURCE = 0x0010;
        internal const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const uint TOKEN_ADJUST_GROUPS = 0x0040;
        internal const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const uint TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        internal const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);
        #endregion

        #region PrivilegeAttributes
        internal const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        internal const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const uint SE_PRIVILEGE_REMOVED = 0x00000004;
        internal const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        #endregion

        #region PrivilegeConstants
        internal const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
        internal const string SE_AUDIT_NAME = "SeAuditPrivilege";
        internal const string SE_BACKUP_NAME = "SeBackupPrivilege";
        internal const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        internal const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
        internal const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
        internal const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        internal const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        internal const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
        internal const string SE_DEBUG_NAME = "SeDebugPrivilege";
        internal const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
        internal const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
        internal const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
        internal const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        internal const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
        internal const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
        internal const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
        internal const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
        internal const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        internal const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        internal const string SE_RELABEL_NAME = "SeRelabelPrivilege";
        internal const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
        internal const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
        internal const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        internal const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
        internal const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        internal const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        internal const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
        internal const string SE_UNDOCK_NAME = "SeUndockPrivilege";
        internal const string SE_UNSOLICITED_INPUT_NAME = "SeUnsolicitedInputPrivilege";
        #endregion

        #region Struct
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LUID
        {
            public int LowPart;
            public uint HighPart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privilege;
        }
        #endregion

        #region P/Invoke Functions
        //提升进程权限的Windows API
        [DllImport(Advapi32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccesss, out IntPtr TokenHandle);

        [DllImport(Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);
        
        [DllImport(Advapi32, SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport(Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges, [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, uint ReturnLength);

        [DllImport(Advapi32, ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref User32Base.TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport(Advapi32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool InitializeSecurityDescriptor(out object pSecurityDescriptor, uint dwRevision);

        [DllImport(Advapi32, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.VariantBool)]
        internal static extern bool SetSecurityDescriptorDacl(ref object pSecurityDescriptor, [MarshalAs(UnmanagedType.VariantBool)] bool bDaclPresent, object pDacl, [MarshalAs(UnmanagedType.VariantBool)] bool bDaclDefaulted);
        #endregion
    }
}
