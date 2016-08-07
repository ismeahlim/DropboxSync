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
    }
}
