using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Helpers
{
    public class AppConfigHelper
    {
        static Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static string GetAppSettingsValue(string key)
        {
            return _config.AppSettings.Settings[key].Value;
        }

        public static void UpdateAppSettingsValue(string key, string value)
        {
            _config.AppSettings.Settings[key].Value = value;
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void AddAppSettingsValue(string newKey, string newValue) 
        {
            _config.AppSettings.Settings.Add(newKey, newValue);
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
