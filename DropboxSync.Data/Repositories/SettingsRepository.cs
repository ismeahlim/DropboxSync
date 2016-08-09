using System;
using DropboxSync.Data.Model;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

namespace DropboxSync.Data.Repositories
{
    public class SettingsRepository
    {
        public List<Settings> GetAll()
        {
            using (var con = new DropBoxSyncContext())
            {
                return con.Settings.ToList();
            }
        }

        public void AddOrUpdate(string key, string value)
        {
            using (var con = new DropBoxSyncContext())
            {
                var setting = con.Settings.Find(key);
                if (setting == null)
                    con.Settings.Add(new Settings()
                    {
                        Key = key,
                        value = value
                    });
                else
                    setting.value = value;

                con.SaveChanges();
            }
        }
    }
}
