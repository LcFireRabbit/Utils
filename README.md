# Windows 的基础功能实现

> **Win32**

**_Advapi32Base_**

OpenProcessToken
LookupPrivilegeValue
AdjustTokenPrivileges
InitializeSecurityDescriptor
SetSecurityDescriptorDacl

**_Advapi32Extension_**

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
