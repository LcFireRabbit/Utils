using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class Kernel32Extension:Kernel32Base
    {
        #region 公共方法
        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="FilePath">INI文件地址</param>
        /// <param name="Section">节名称</param>
        /// <param name="Key">键值</param>
        /// <param name="Value">值</param>
        public static void WriteINIValue(string FilePath, string Section, string Key, string Value)
        {
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }
            int n = WritePrivateProfileString(Section, Key, Value, FilePath);
        }

        /// <summary>
        /// 读INI文件
        /// </summary>
        /// <param name="FilePath">INI文件地址</param>
        /// <param name="Section">节名称</param>
        /// <param name="Key">键值</param>
        /// <returns></returns>
        public static string ReadINIValue(string FilePath, string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(4096);
            GetPrivateProfileString(Section, Key, "", temp, 4096, FilePath);
            return temp.ToString();
        }

        /// <summary>
        /// 获取本地时区
        /// </summary>
        /// <returns></returns>
        public static string GetLocalTimeZone()
        {
            // 检测当前系统是否为旧系统
            if (Common.IsOldOsVersion())
            {
                TimeZoneInformation tzi = new TimeZoneInformation();
                GetTimeZoneInformation(ref tzi);
                return TimeZoneInfo2CustomString(tzi);
            }
            DynamicTimeZoneInformation dtzi = new DynamicTimeZoneInformation();
            GetDynamicTimeZoneInformation(ref dtzi);
            return DynamicTimeZoneInfo2CustomString(dtzi);
        }

        /// <summary>
        /// 设置本地时区
        /// 参数取值"China Standard Time"，即可设置为中国时区
        /// </summary>
        /// <param name="timeZoneName_en"></param>
        /// <returns></returns>
        public static bool SetLocalTimeZone(string timeZoneName_en)
        {
            if (Advapi32Extension.GrantPrivilege(Advapi32Base.SE_TIME_ZONE_NAME))
            {
                DynamicTimeZoneInformation dtzi = TimeZoneName2DynamicTimeZoneInformation(timeZoneName_en);
                bool success = false;
                // 检测当前系统是否为旧系统
                if (Common.IsOldOsVersion())
                {
                    TimeZoneInformation tzi = DynamicTimeZoneInformation2TimeZoneInformation(dtzi);
                    success = SetTimeZoneInformation(ref tzi);
                }
                else
                {
                    success = SetDynamicTimeZoneInformation(ref dtzi);
                }
                if (success)
                {
                    TimeZoneInfo.ClearCachedData();  // 清除缓存
                }
                if (!Advapi32Extension.RevokePrivilege(Advapi32Base.SE_TIME_ZONE_NAME))
                {
                    // 撤权失败
                }
                return success;
            }
            // 授权失败
            return false;
        }

        /// <summary>
        /// 获取本地时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLocalTime()
        {
            SystemTime sysTime = new SystemTime();
            GetLocalTime(ref sysTime);
            return SystemTime2DateTime(sysTime);
        }

        /// <summary>
        /// 设置本地时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool SetLocalTime(SystemTime dateTime)
        {
            bool success = SetLocalTime(ref dateTime);
            return success;
        }

        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetSystemTime()
        {
            SystemTime sysTime = new SystemTime();
            GetSystemTime(ref sysTime);
            return SystemTime2DateTime(sysTime);
        }

        /// <summary>
        /// 设置系统时间（UTC）
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool SetSystemTime(DateTime dateTime)
        {
            SystemTime sysTime = DateTime2SystemTime(dateTime);
            bool success = SetSystemTime(ref sysTime);
            return success;
        }

        /// <summary>
        /// 将SystemTime转换为DateTime
        /// </summary>
        /// <param name="sysTime"></param>
        /// <returns></returns>
        public static DateTime SystemTime2DateTime(SystemTime sysTime)
        {
            return new DateTime(sysTime.year, sysTime.month, sysTime.day, sysTime.hour, sysTime.minute, sysTime.second, sysTime.milliseconds);
        }

        /// <summary>
        /// 将DateTime转换为SystemTime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static SystemTime DateTime2SystemTime(DateTime dateTime)
        {
            SystemTime sysTime = new SystemTime
            {
                year = Convert.ToUInt16(dateTime.Year),
                month = Convert.ToUInt16(dateTime.Month),
                day = Convert.ToUInt16(dateTime.Day),
                hour = Convert.ToUInt16(dateTime.Hour),
                minute = Convert.ToUInt16(dateTime.Minute),
                second = Convert.ToUInt16(dateTime.Second),
                milliseconds = Convert.ToUInt16(dateTime.Millisecond)
            };
            return sysTime;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 将TimeZoneInformation转换为自定义string
        /// </summary>
        /// <param name="tzi"></param>
        /// <returns></returns>
        private static string TimeZoneInfo2CustomString(TimeZoneInformation tzi)
        {
            return tzi.standardName + "(" + tzi.bias + ")";
        }

        /// <summary>
        /// 将DynamicTimeZoneInformation转换为自定义string
        /// </summary>
        /// <param name="dtzi"></param>
        /// <returns></returns>
        private static string DynamicTimeZoneInfo2CustomString(DynamicTimeZoneInformation dtzi)
        {
            return dtzi.standardName + "(" + dtzi.bias + ")";
        }

        /// <summary>
        /// 根据时区名获取对应的DynamicTimeZoneInformation
        /// </summary>
        /// <param name="timeZoneName"></param>
        /// <returns></returns>
        private static DynamicTimeZoneInformation TimeZoneName2DynamicTimeZoneInformation(string timeZoneName)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);

            DynamicTimeZoneInformation dtzi = new DynamicTimeZoneInformation
            {
                standardName = timeZoneInfo.StandardName,
                standardDate = new SystemTime(),
                daylightName = timeZoneInfo.DaylightName,
                daylightDate = new SystemTime(),
                timeZoneKeyName = timeZoneInfo.Id,
                dynamicDaylightTimeDisabled = false,
                bias = -Convert.ToInt32(timeZoneInfo.BaseUtcOffset.TotalMinutes)
            };
            return dtzi;
        }

        /// <summary>
        /// 将DynamicTimeZoneInformation转换为TimeZoneInformation
        /// </summary>
        /// <param name="dtzi"></param>
        /// <returns></returns>
        private static TimeZoneInformation DynamicTimeZoneInformation2TimeZoneInformation(DynamicTimeZoneInformation dtzi)
        {
            return new TimeZoneInformation
            {
                bias = dtzi.bias,
                standardName = dtzi.standardName,
                standardDate = dtzi.standardDate,
                standardBias = dtzi.standardBias,
                daylightName = dtzi.daylightName,
                daylightDate = dtzi.daylightDate,
                daylightBias = dtzi.daylightBias
            };
        }
        #endregion
    }
}
