using System;
using System.Data.Entity;

using DropboxSync.Data.Model;

namespace DropboxSync.Data
{
    public class DropBoxSyncContext : DbContext, IDisposable
    {
        public DbSet<Settings> Settings { get; set; }

        public DbSet<DropboxRef> DropboxRef { get; set; }
    }
}
