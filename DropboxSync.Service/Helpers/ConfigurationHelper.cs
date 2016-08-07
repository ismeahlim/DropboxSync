using System;
using System.Configuration;

namespace DropboxSync.Service.Helpers
{
    public class ConfigurationHelper
    {
        public static T GetValue<T>(string key, T defaultValue)
        {
            T res = defaultValue;

            try
            {
                string val = ConfigurationManager.AppSettings[key].ToString();
                res = (T)Convert.ChangeType(val, typeof(T));
            }
            catch { }

            return res;
        }
        public static T GetValue<T>(string key)
        {
            return GetValue<T>(key, default(T));
        }
        public static string GetValue(string key)
        {
            return GetValue<string>(key, string.Empty);
        }
    }
}
