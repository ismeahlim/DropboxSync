using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NLog;

using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Models;
using ExactOnline.Client.OAuth;

using DotNetOpenAuth.OAuth2;

namespace DropboxSync.Service.ExactOnline
{
    public class ExactOnlineSync
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private string _apiKey;
        private string _apiSecret;
        private string _callbackUrl;
        private string _endPoint; 
        private string _accessToken;

        private DateTime? _accessTokenExpiration;

        private readonly UserAuthorization _userAuthorization;

        public delegate void UpdateTokenDelegate(string token, DateTime? tokenExpired);
        private UpdateTokenDelegate _updateToken;

        //private int _timeoutInMinutes = 20;
        //private int _timeoutReadWriteInSec = 10;

        private const string CATEGORY_GENERAL = "3b6d3833-b31b-423d-bc3c-39c62b8f2b12";
        private const int DOCUMENTTYPE_MISCELLANEOUS = 55;

        private ExactOnlineClient _client;

        public ExactOnlineSync(string apiKey, string apiSecret, string callbackUrl,
            string endPoint, string accessToken, DateTime? accessTokenExpiration,
            UpdateTokenDelegate updateToken)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _callbackUrl = callbackUrl;
            _endPoint = endPoint; 
            _accessToken = accessToken;
            _accessTokenExpiration = accessTokenExpiration;
            _updateToken = updateToken;

            //_timeoutInMinutes = timeoutInMinutes;
            //_timeoutReadWriteInSec = timeoutReadWriteInMinutes;

            _userAuthorization = new UserAuthorization();
        }

        public bool Initialize()
        {
            _logger.Debug("ExactOnlineSync.Initialize started.");

            InitializeAuth();
            _client = new ExactOnlineClient(_endPoint, this.GetAccessToken);
            
            _logger.Debug("DropBoxSync.Initialize successful");
            return true;
        }

        private void InitializeAuth()
        {
            if (IsAccessTokenValid())
            {
                _userAuthorization.AuthorizationState = new AuthorizationState
                {
                    Callback = new Uri(_callbackUrl)
                };

                _userAuthorization.AuthorizationState.AccessToken = _accessToken;
                _userAuthorization.AuthorizationState.AccessTokenExpirationUtc = _accessTokenExpiration;
            }
        }
        private bool IsAccessTokenValid()
        {
            return !string.IsNullOrWhiteSpace(_accessToken)
                && _accessTokenExpiration.HasValue
                && _accessTokenExpiration.Value > DateTime.UtcNow;
        }

        private string GetAccessToken()
        {
            UserAuthorizations.Authorize(_userAuthorization, _endPoint, _apiKey, _apiSecret,
                new Uri(_callbackUrl));

            if (_updateToken != null 
                && _userAuthorization.AccessToken != null
                && _accessToken != _userAuthorization.AccessToken)
            {
                _accessToken = _userAuthorization.AccessToken;
                _accessTokenExpiration = _userAuthorization.AuthorizationState.AccessTokenExpirationUtc;

                _updateToken(_accessToken, _accessTokenExpiration);
            }

            return _userAuthorization.AccessToken;
        }

        public List<Guid> GetDocumentIds()
        {
            return _client.For<Document>().Select(new string[] { "ID" }).Get()
                .Select(x => x.ID).ToList();
        }

        public bool Delete<T>(T entity) where T : class
        {
            try
            {
                return _client.For<T>().Delete(entity);
            }
            catch (Exception ex)
            {
                _logger.Error("ExactOnlineSync.Delete: ", ex.Message);
                return false;
            }
        }
        public bool DeleteDocument(Guid id)
        {
            try
            {
                var doc = _client.For<Document>()
                    .GetEntity(id);
                return _client.For<Document>().Delete(doc);
            }
            catch (Exception ex)
            {
                _logger.Error("ExactOnlineSync.Delete: ", ex.Message);
                return false;
            }

        }

        public bool Create<T>(ref T entity) where T : class
        {
            return _client.For<T>().Insert(ref entity);
        }
        public Guid CreateDocument(string subject, string body)
        {
            var document = new Document
            {
                Subject = subject,
                Body = body,
                Category = Guid.Parse(CATEGORY_GENERAL),
                Type = DOCUMENTTYPE_MISCELLANEOUS,
                DocumentDate = DateTime.UtcNow.Date
            };

            if (Create(ref document))
            {
                return document.ID;
            }

            return Guid.Empty;
        }

        public bool UpdateDocument(Guid id, string body)
        {
            var document = _client.For<Document>().GetEntity(id);
            if (document == null)
                throw new Exception("ExactOnlineSync.UpdateDocument document get empty");

            document.Body = body;
            document.DocumentDate = DateTime.UtcNow.Date;

            return _client.For<Document>().Update(document);
        }
 

    }
}
