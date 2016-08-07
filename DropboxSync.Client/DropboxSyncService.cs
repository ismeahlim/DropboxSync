using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;

using DropboxSync.Service;

namespace DropboxSync.Client
{
    public class DropboxSyncService : IService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DropboxService _dropboxService;

        public DropboxSyncService()
        {
            _dropboxService = new DropboxService();
        }

        public void Start()
        {
            _logger.Info("Service starting...");

            _dropboxService.DoSync();

            _logger.Info("Service started.");
        }

        public void Stop()
        {
            _logger.Info("Service stopped.");
        }
    }
}
