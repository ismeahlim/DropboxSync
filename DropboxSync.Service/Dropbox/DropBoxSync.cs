using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using System.Windows;
using System.Net;
using System.Net.Http;
using System.IO;
using NLog;

using Dropbox.Api;
using Dropbox.Api.Files;

namespace DropboxSync.Service.Dropbox
{
    public class DropBoxSync
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private string _apiKey;
        private string _apiSecret;
        private string _accessToken;
        private string _path;

        private int _timeoutInMinutes = 20;
        private int _timeoutReadWriteInSec = 10;

        private DropboxClient _client;
        private string _currentCursor;

        public DropBoxSync(string apiKey, string apiSecret, string accessToken, string path,
            int timeoutInMinutes, int timeoutReadWriteInMinutes)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _accessToken = accessToken;
            _path = path;

            _timeoutInMinutes = timeoutInMinutes;
            _timeoutReadWriteInSec = timeoutReadWriteInMinutes;
        }

        public bool Initialize()
        {
            _logger.Debug("DropBoxSync.Initialize started.");

            DropboxCertHelper.InitializeCertPinning();

            // Specify socket level timeout which decides maximum waiting time when on bytes are
            // received by the socket.
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                // Specify request level timeout which decides maximum time taht can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            };

            try
            {
                var config = new DropboxClientConfig("DropBoxSync")
                {
                    HttpClient = httpClient
                };

                _client = new DropboxClient(_accessToken, config);
            }
            catch (HttpException e)
            {
                _logger.Error("Exception reported from RPC layer");
                _logger.Error("    Status code: {0}", e.StatusCode);
                _logger.Error("    Message    : {0}", e.Message);
                if (e.RequestUri != null)
                {
                    _logger.Error("    Request uri: {0}", e.RequestUri);
                }
            }

            _logger.Debug("DropBoxSync.Initialize successful");
            return true;
        }

        public async Task<string> GetAccessToken()
        {
            _logger.Info("GetAccessToken: Waiting for credentials...");
            var completion = new TaskCompletionSource<Tuple<string, string>>();

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var login = new DropBoxLoginForm(_apiKey);
                    app.Run(login);
                    if (login.Result)
                    {
                        completion.TrySetResult(Tuple.Create(login.AccessToken, login.Uid));
                    }
                    else
                    {
                        completion.TrySetCanceled();
                    }
                }
                catch (Exception e)
                {
                    completion.TrySetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                var result = await completion.Task;
                _logger.Info("GetAccessToken: Result Received.");

                _accessToken = result.Item1;
                var uid = result.Item2;
                _logger.Debug("GetAccessToken: Uid: {0}", uid);
            }
            catch (Exception e)
            {
                e = e.InnerException ?? e;
                _logger.Error("GetAccessToken Error: {0}", e.Message);
                return null;
            }

            return _accessToken;
        }

        public async Task<ListFolderResult> ListFolder()
        {
            return await _client.Files.ListFolderAsync(_path);
        }

        public async Task<bool> TryCreateFolder()
        {
            try
            {
                await _client.Files.CreateFolderAsync(new CreateFolderArg(_path));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug("DropBoxSync.TryCreateFolder Error:{0}", ex.Message);
                return false;
            }
        }

        public async Task<List<DropboxFileInfo>> ListAllFiles()
        {
            var list =
                (_currentCursor == "")
                ? await _client.Files.ListFolderContinueAsync(_currentCursor)
                : await _client.Files.ListFolderAsync(_path);
            
            _currentCursor = list.Cursor;
            //LogMetaData(list.Entries);

            return list.Entries.Where(i => i.IsFile)
                .Select(i => new DropboxFileInfo() 
                {
                    Id = i.AsFile.Id,
                    Name = i.Name,
                    Path = i.PathLower,
                    ServerModified = i.AsFile.ServerModified
                })
                .ToList();
        }

        public async Task<bool> LongPoll()
        {
            var result = await _client.Files.ListFolderLongpollAsync(_currentCursor);
            return result.Changes;
        }

        public async Task<string> GetLatestCursor()
        {
            var result = await _client.Files.ListFolderGetLatestCursorAsync(_path);
            _currentCursor = result.Cursor;

            return _currentCursor;
        }

        private bool LogMetaData(IList<Metadata> list)
        {
            foreach (var item in list)
            {
                var file = item.AsFile;
                _logger.Info("item:{0}|{1}", file.Id, file.Name, (file.IsDeleted));
            }
            return true;
        }

        public async Task<string> Download(string filePath)
        {
            try
            {
                using (var response = await _client.Files.DownloadAsync(filePath))
                {
                    return await response.GetContentAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("DropboxSync.Download: {0}", ex.Message);
                return "";
            }

        }

    }
}
