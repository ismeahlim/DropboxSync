using System;
using System.Collections.Generic;
using System.Linq;

using NLog;
using DropboxSync.Data.Repositories;
using DropboxSync.Data.Model;

using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;

using DropboxSync.Service.Dropbox;
using DropboxSync.Service.ExactOnline;

namespace DropboxSync.Service
{
    public class DropboxService 
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private DropBoxSync _dropBoxSync;
        private ExactOnlineSync _exactOnlineSync;

        private DropboxRefRepository _dropboxRefRepo;
        private DropboxServiceSettings _dropboxServiceSettings;

        public DropboxService()
        {
            _dropboxRefRepo = new DropboxRefRepository();
            _dropboxServiceSettings = new DropboxServiceSettings();
        }

        public void DoSync()
        {
            _dropboxServiceSettings.LoadFromDb();

            Initiailize();

            Task.Run((Func<Task<bool>>)this.SyncDropbox);
        }

        private void Initiailize()
        {
            _dropboxServiceSettings.LoadFromDb();

            _exactOnlineSync = new ExactOnlineSync(_dropboxServiceSettings.ExactApiKey, _dropboxServiceSettings.ExactApiSecret, _dropboxServiceSettings.ExactCallbackUrl, _dropboxServiceSettings.ExactEndPoint, _dropboxServiceSettings.ExactAccessToken);
            _exactOnlineSync.Initialize();

            _dropBoxSync = new DropBoxSync(_dropboxServiceSettings.DropBoxApiKey, _dropboxServiceSettings.DropBoxApiSecret, _dropboxServiceSettings.DropBoxAccessToken, _dropboxServiceSettings.DropBoxPath, _dropboxServiceSettings.DropBoxTimeout, _dropboxServiceSettings.DropBoxReadWriteTimeout);
            _dropBoxSync.Initialize();
        }

        private async Task<bool> SyncDropbox()
        {
            //Try create to make sure root path exists
            await _dropBoxSync.TryCreateFolder();

            while (true)
            {
                //Get from dropbox all file lists
                var dropboxLists = await _dropBoxSync.ListAllFiles();

                //Get from db all references
                var dropboxRefs = _dropboxRefRepo.GetAll();

                //Get all available document ids
                var exactDocIds = _exactOnlineSync.GetDocumentIds();

                //delete
                DoDeleteFiles(dropboxRefs, dropboxLists, exactDocIds);

                //update and upload
                await DoUpdateFiles(dropboxRefs, dropboxLists);

                //add and upload
                await DoAddFiles(dropboxRefs, dropboxLists);

                await DoLongPoll();
            }
        }

        private void DoDeleteFiles(List<DropboxRef> dropboxRefs, 
            List<DropboxFileInfo> dropboxLists,
            List<Guid> exactDocIds)
        {
            var dropboxDelLists = dropboxRefs
                .Where(r => !dropboxLists.Any(d => d.Id == r.DropboxId));
            foreach (var item in dropboxDelLists)
            {
                DoDeleteFile(item, exactDocIds);
            }
        }
        private void DoDeleteFile(DropboxRef item, List<Guid> exactDocIds)
        {
            _logger.Info("Dropbox Deleting d:{0} e:{1}", item.DropboxId, item.ExactId);

            try
            {
                bool delSuccess = true;

                var exactIdStr = Guid.Parse(item.ExactId);
                //if exact not containing the file, not need to do delete from exactonline
                if (exactDocIds.Contains(exactIdStr))
                    delSuccess = _exactOnlineSync.DeleteDocument(exactIdStr);

                //delete from db
                if (delSuccess)
                    delSuccess = _dropboxRefRepo.Delete(item.Id);

                _logger.Info("Dropbox Deleted d:{0} e:{1} r:{2}", item.DropboxId, item.ExactId, delSuccess);
            }
            catch (Exception ex)
            {
                _logger.Error("Dropbox Delete Error: d:{0} e:{1} err:{2}", item.DropboxId, item.ExactId,
                    ex.Message);
            }
        }

        private async Task<bool> DoAddFiles(List<DropboxRef> dropboxRefs,
            List<DropboxFileInfo> dropboxLists)
        {
            var dropboxAddLists = dropboxLists
                  .Where(d => !dropboxRefs.Any(r => r.DropboxId == d.Id));
            foreach (var item in dropboxAddLists)
            {
                await DoAddFile(item);
            }

            return true;
        }
        private async Task<bool> DoAddFile(DropboxFileInfo item)
        {
            _logger.Info("Dropbox Adding id:{0} n:{1}", item.Id, item.Name);

            try
            {
                //dropbox Download
                var body = await _dropBoxSync.Download(item.Path);

                //Exact add document and upload
                var exactId = _exactOnlineSync.CreateDocument(item.Name, body);

                //Db Add
                var exactIdStr = exactId.ToString();
                _dropboxRefRepo.Add(item.Id, exactIdStr, item.ServerModified);

                _logger.Info("Dropbox Added d:{0} n:{1} e:{2}", item.Id, item.Name, exactIdStr);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Dropbox Add Error d:{0} n:{1} err:{2}", item.Id, item.Name, ex.Message);
                return false;
            }
        }

        private async Task<bool> DoUpdateFiles(List<DropboxRef> dropboxRefs,
            List<DropboxFileInfo> dropboxLists)
        {
            var list = dropboxRefs
                .Where(r => dropboxLists.Any(d =>
                    d.Id == r.DropboxId
                    && d.ServerModified != r.DropboxModifiedOn
                ));
            foreach (var rItem in list)
            {
                var dItem = dropboxLists.Where(d => d.Id == rItem.DropboxId)
                    .FirstOrDefault();

                await DoUpdateFile(dItem, rItem);
            }

            return true;
        }
        private async Task<bool> DoUpdateFile(DropboxFileInfo item, DropboxRef dropboxRef)
        {
            _logger.Info("Dropbox Updating id:{0} n:{1} e:{2}", item.Id, item.Name, dropboxRef.ExactId);

            try
            {
                //dropbox Download
                var body = await _dropBoxSync.Download(item.Path);

                var exactId = Guid.Parse(dropboxRef.ExactId);
                //Exact update document and upload
                _exactOnlineSync.UpdateDocument(exactId, body);

                //Db Update
                _dropboxRefRepo.Update(dropboxRef.Id, item.ServerModified);

                _logger.Info("Dropbox Updated d:{0} n:{1} e:{2}", item.Id, item.Name, dropboxRef.ExactId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Dropbox Update Error d:{0} n:{1} e:{2} err:{3}", item.Id, item.Name, dropboxRef.ExactId, ex.Message);
                return false;
            }
        }

        private async Task<bool> DoLongPoll()
        {
            await _dropBoxSync.GetLatestCursor();

            bool isChanged = await _dropBoxSync.LongPoll();
            _logger.Debug("WaitForChanges:Result:{0}", isChanged);

            return true;
        }
    }
}
