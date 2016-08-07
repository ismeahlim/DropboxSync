using System;
using System.Collections.Generic;

namespace DropboxSync.Service.Dropbox
{
    public class DropboxFileInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime ServerModified { get; set; }
    }
}
