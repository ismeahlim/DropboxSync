using System;
using System.Collections.Generic;

using DropboxSync.Data.Repositories;

namespace DropboxSync.Service.Helpers
{
    public class SettingsHelper
    {
        public static Dictionary<string, string> GetFromDb()
        {
            var res = new Dictionary<string, string>();

            var repo = new SettingsRepository();
            var list = repo.GetAll();

            list.ForEach(t => res.Add(t.Key, t.value));

            return res;
        }

        public static T GetValue<T>(Dictionary<string, string> dict, string key, T defaultValue)
        {
            T res = defaultValue;

            try
            {
                string val = dict[key].ToString();
                res = (T)Convert.ChangeType(val, typeof(T));
            }
            catch { }

            return res;
        }

        public static T GetValue<T>(Dictionary<string, string> dict, string key)
        {
            return GetValue<T>(dict, key, default(T));
        }
        public static string GetValue(Dictionary<string, string> dict, string key)
        {
            return GetValue<string>(dict, key, string.Empty);
        }
    }
}
