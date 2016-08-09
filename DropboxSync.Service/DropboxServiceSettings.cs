using System;
using DropboxSync.Service.Helpers;

namespace DropboxSync.Service
{
    public class DropboxServiceSettings
    {
        public string DropBoxApiKey { get; private set; }
        public string DropBoxApiSecret { get; private set; }
        public string DropBoxAccessToken { get; private set; }
        public string DropBoxPath { get; private set; }

        public int DropBoxTimeout { get; private set; }
        public int DropBoxReadWriteTimeout { get; private set; }

        public string ExactApiKey { get; private set; }
        public string ExactApiSecret { get; private set; }
        public string ExactCallbackUrl { get; private set; }
        public string ExactAccessToken { get; private set; }
        public string ExactEndPoint { get; private set; }

        public DateTime? ExactTokenExpiration { get; private set; }

        public DropboxServiceSettings() { }

        public void LoadFromAppSettings()
        {
            DropBoxApiKey = ConfigurationHelper.GetValue("DropBox:ApiKey");
            DropBoxApiSecret = ConfigurationHelper.GetValue("DropBox:ApiSecret");
            DropBoxAccessToken = ConfigurationHelper.GetValue("DropBox:AccessToken");
            DropBoxPath = ConfigurationHelper.GetValue("DropBox:Path");
            DropBoxTimeout = ConfigurationHelper.GetValue<int>("DropBox:Timeout", 20);
            DropBoxReadWriteTimeout = ConfigurationHelper.GetValue<int>("DropBox:ReadWriteTimeout", 10);

            ExactApiKey = ConfigurationHelper.GetValue("Exact:ApiKey");
            ExactApiSecret = ConfigurationHelper.GetValue("Exact:ApiSecret");
            ExactCallbackUrl = ConfigurationHelper.GetValue("Exact:CallbackUrl");
            ExactAccessToken = ConfigurationHelper.GetValue("Exact:AccessToken");
            ExactEndPoint = ConfigurationHelper.GetValue("Exact:EndPoint", "https://start.exactonline.nl");
        }

        public void LoadFromDb()
        {
            var dict = SettingsHelper.GetFromDb();

            DropBoxApiKey = SettingsHelper.GetValue(dict, "DropBox:ApiKey");
            DropBoxApiSecret = SettingsHelper.GetValue(dict, "DropBox:ApiSecret");
            DropBoxAccessToken = SettingsHelper.GetValue(dict, "DropBox:AccessToken");
            DropBoxPath = SettingsHelper.GetValue(dict, "DropBox:Path");
            DropBoxTimeout = SettingsHelper.GetValue<int>(dict, "DropBox:Timeout", 20);
            DropBoxReadWriteTimeout = SettingsHelper.GetValue<int>(dict, "DropBox:ReadWriteTimeout", 10);

            ExactApiKey = SettingsHelper.GetValue(dict, "Exact:ApiKey");
            ExactApiSecret = SettingsHelper.GetValue(dict, "Exact:ApiSecret");
            ExactCallbackUrl = SettingsHelper.GetValue(dict, "Exact:CallbackUrl");
            ExactAccessToken = SettingsHelper.GetValue(dict, "Exact:AccessToken");
            ExactEndPoint = SettingsHelper.GetValue(dict, "Exact:EndPoint", "https://start.exactonline.nl");
            ExactTokenExpiration = SettingsHelper.GetValue<DateTime>(dict, "Exact:AccessTokenExpiration");
            if (ExactTokenExpiration == DateTime.MinValue)
                ExactTokenExpiration = null;
        }
    }
}
