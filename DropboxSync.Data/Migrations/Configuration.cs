using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;

using System.Collections.Generic;
using DropboxSync.Data.Model;

namespace DropboxSync.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DropBoxSyncContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DropBoxSyncContext context)
        {
            var settings = new List<Settings>()
            {
                new Settings { Key="DropBox:ApiKey", value="at1zopsx37xc5c7"},
                new Settings { Key="DropBox:ApiSecret", value="ubg8rv41e684wlk"},
                new Settings { Key="DropBox:AccessToken", value="b4-XqjP0pKEAAAAAAABW8sM4y5uJt8u9EPdD0kg1v5sxYfGdCwEUbHg4gcAD_4Id"},
                new Settings { Key="DropBox:Path", value="/Test/"},
                new Settings { Key="DropBox:Timeout", value="20"},
                new Settings { Key="DropBox:ReadWriteTimeout", value="10"},

                new Settings { Key="Exact:ApiKey", value="f131c3ad-ff00-4569-8edf-eff90f5d473a"},
                new Settings { Key="Exact:ApiSecret", value="hrfXr8D6Cusq"},
                new Settings { Key="Exact:CallbackUrl", value="https://YOUR_NAMESPACE/login/callback"},
                new Settings { Key="Exact:EndPoint", value="https://start.exactonline.co.uk"},
                new Settings { Key="Exact:AccessToken", value=""},
                new Settings { Key="Exact:RefreshToken", value=""}
            };

            settings.ForEach(m => context.Settings.AddOrUpdate(x => x.Key, m));
            context.SaveChanges();
        }
    }
}
