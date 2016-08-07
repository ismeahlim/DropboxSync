using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Topshelf;
using DropboxSync.Service.Helpers;

namespace DropboxSync.Client
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            HostFactory.Run(configuration =>
            {

                configuration.RunAsLocalSystem();

                configuration.SetDescription(ConfigurationHelper.GetValue("Service:SetDescription", "Exact Sync Dropbox Client"));
                configuration.SetDisplayName(ConfigurationHelper.GetValue("Service:SetDisplayName", "Exact.Sync.Dropbox.Client"));
                configuration.SetServiceName(ConfigurationHelper.GetValue("Service:SetServiceName", "Exact.Sync.Dropbox.Client"));

                configuration.Service<IService>(service =>
                {
                    service.ConstructUsing(x => new DropboxSyncService());
                    service.WhenStarted(x => x.Start());
                    service.WhenStopped(x => x.Stop());
                });
            });
        }
    }
}
