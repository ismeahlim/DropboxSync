using System;
using DropboxSync.Data.Model;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;

namespace DropboxSync.Data.Repositories
{
    public class DropboxRefRepository
    {
        public List<DropboxRef> GetAll()
        {
            using (var con = new DropBoxSyncContext())
            {
                return con.DropboxRef.ToList();
            }
        }

        public async Task<bool> AddSync(DropboxRef item)
        {
            using (var con = new DropBoxSyncContext()) 
            {
                con.DropboxRef.Add(item);
                await con.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> AddSync(string dropboxId, string exactId, DateTime dropboxModifiedOn)
        {
            return await this.AddSync(new DropboxRef()
            {
                Id = Guid.NewGuid(),
                DropboxId = dropboxId,
                ExactId = exactId,
                DropboxModifiedOn = dropboxModifiedOn,
                ModifiedOn = DateTime.UtcNow
            });
        }

        public void Add(DropboxRef item)
        {
            using (var con = new DropBoxSyncContext())
            {
                con.DropboxRef.Add(item);
                con.SaveChanges();
            }
        }

        public void Add(string dropboxId, string exactId, DateTime dropboxModifiedOn)
        {
            this.Add(new DropboxRef()
            {
                Id = Guid.NewGuid(),
                DropboxId = dropboxId,
                ExactId = exactId,
                DropboxModifiedOn = dropboxModifiedOn,
                ModifiedOn = DateTime.UtcNow
            });
        }

        public void Update(Guid id, DateTime dropboxModifiedOn)
        {
            using (var con = new DropBoxSyncContext())
            {
                var dropboxRef = con.DropboxRef.Find(id);
                if (dropboxRef == null)
                    throw new ArgumentNullException("DropboxRefRepository.Update.Find empty");

                dropboxRef.DropboxModifiedOn = dropboxModifiedOn;
                dropboxRef.ModifiedOn = DateTime.UtcNow;

                con.SaveChanges();
            }
        }

        public bool Delete(Guid id)
        {
            using (var con = new DropBoxSyncContext())
            {
                try
                {
                    var success = con.Database.ExecuteSqlCommand(
                        "Delete DropboxRefs where id = {0}", id.ToString());

                    return success > 0;
                }
                catch ( Exception ex) 
                {
                    return false;
                }

            }
        }
    }
}
