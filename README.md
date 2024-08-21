# Windows的基础功能实现
>
> **Win32**

***Advapi32Base***
OpenProcessToken
LookupPrivilegeValue
AdjustTokenPrivileges
InitializeSecurityDescriptor
SetSecurityDescriptorDacl

***Advapi32Extension***
```C#
/// <summary>
/// 授予权限
/// 参数取值为文件PrivilegeAPI中的类PrivilegeConstants中的字段
/// </summary>
/// <param name="privilegeName">PrivilegeConstants类中的字段</param>
GrantPrivilege(string privilegeName)

/// <summary>
/// 撤销权限
/// 参数取值为文件PrivilegeAPI中的类PrivilegeConstants中的字段
/// </summary>
/// <param name="privilegeName">PrivilegeConstants类中的字段</param>
/// <returns></returns>
RevokePrivilege(string privilegeName)
```