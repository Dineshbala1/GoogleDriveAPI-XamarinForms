using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Utilities;

namespace XamarinFormsGoogleDriveAPI.Droid.Renderer
{
    public class RefreshOAuth2Authenticator : WebRedirectAuthenticator

    {
        string clientId;
        string clientSecret;
        string scope;
        Uri authorizeUrl;
        Uri accessTokenUrl;
        Uri redirectUrl;
        GetUsernameAsyncFunc getUsernameAsync;

        string requestState;
        bool reportedForgery;

        public string ClientId
        {
            get { return clientId; }
        }

        public string ClientSecret
        {
            get { return clientSecret; }
        }

        public string Scope
        {
            get { return scope; }
        }

        public Uri AuthorizeUrl
        {
            get { return authorizeUrl; }
        }

        public Uri RedirectUrl
        {
            get { return redirectUrl; }
        }

        public Uri AccessTokenUrl
        {
            get { return accessTokenUrl; }
        }

        public RefreshOAuth2Authenticator(string clientId, string scope, Uri authorizeUrl, Uri redirectUrl, GetUsernameAsyncFunc getUsernameAsync = null)
            : this(redirectUrl)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("clientId must be provided", nameof(clientId));
            }
            this.clientId = clientId;

            this.scope = scope ?? "";

            if (authorizeUrl == null)
            {
                throw new ArgumentNullException(nameof(authorizeUrl));
            }
            this.authorizeUrl = authorizeUrl;

            this.getUsernameAsync = getUsernameAsync;

            this.redirectUrl = redirectUrl;

            accessTokenUrl = null;
        }

        public RefreshOAuth2Authenticator(string clientId, string clientSecret, string scope, Uri authorizeUrl, Uri redirectUrl, Uri accessTokenUrl, GetUsernameAsyncFunc getUsernameAsync = null)
            : this(redirectUrl, clientSecret, accessTokenUrl)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("clientId must be provided", nameof(clientId));
            }
            this.clientId = clientId;

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException("clientSecret must be provided", nameof(clientSecret));
            }
            this.clientSecret = clientSecret;

            this.scope = scope ?? "";

            if (authorizeUrl == null)
            {
                throw new ArgumentNullException(nameof(authorizeUrl));
            }
            this.authorizeUrl = authorizeUrl;

            if (accessTokenUrl == null)
            {
                throw new ArgumentNullException(nameof(accessTokenUrl));
            }
            this.accessTokenUrl = accessTokenUrl;

            this.redirectUrl = redirectUrl;

            this.getUsernameAsync = getUsernameAsync;
        }

        RefreshOAuth2Authenticator(Uri redirectUrl, string clientSecret = null, Uri accessTokenUrl = null)
            : base(redirectUrl, redirectUrl)
        {
            this.clientSecret = clientSecret;

            this.accessTokenUrl = accessTokenUrl;

            this.redirectUrl = redirectUrl;

            //
            // Generate a unique state string to check for forgeries
            //
            var chars = new char[16];
            var rand = new Random();
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)rand.Next('a', 'z' + 1);
            }
            requestState = new string(chars);
        }

        bool IsImplicit => accessTokenUrl == null;

        public override Task<Uri> GetInitialUrlAsync()
        {
            var url = new Uri(string.Format(
                "{0}?client_id={1}&redirect_uri={2}&response_type={3}&scope={4}&state={5}&access_type=offline&prompt=consent",
                authorizeUrl.AbsoluteUri,
                Uri.EscapeDataString(clientId),
                Uri.EscapeDataString(RedirectUrl.AbsoluteUri),
                IsImplicit ? "token" : "code",
                Uri.EscapeDataString(scope),
                Uri.EscapeDataString(requestState)));

            var tcs = new TaskCompletionSource<Uri>();
            tcs.SetResult(url);
            return tcs.Task;
        }

        public virtual async Task<IDictionary<string, string>> RequestRefreshTokenAsync(string refreshToken)
        {
            var queryValues = new Dictionary<string, string>
            {
                {"refresh_token", refreshToken},
                {"client_id", ClientId},
                {"grant_type", "refresh_token"}
            };

            if (!string.IsNullOrEmpty(ClientSecret))
            {
                queryValues["client_secret"] = ClientSecret;
            }

            try
            {
                var accountProperties = await RequestAccessTokenAsync(queryValues).ConfigureAwait(false);

                OnRetrievedAccountProperties(accountProperties);

                return accountProperties;
            }
            catch (Exception e)
            {
                OnError(e);

                throw;
                    // maybe don't need this? this will throw the exception in order to maintain backward compatibility, but maybe could just return -1 or something instead?
            }
        }

        protected override void OnPageEncountered(Uri url, IDictionary<string, string> query, IDictionary<string, string> fragment)
        {
            var all = new Dictionary<string, string>(query);
            foreach (var kv in fragment)
                all[kv.Key] = kv.Value;

            //
            // Check for forgeries
            //
            if (all.ContainsKey("state"))
            {
                if (all["state"] != requestState && !reportedForgery)
                {
                    reportedForgery = true;
                    OnError("Invalid state from server. Possible forgery!");
                    return;
                }
            }

            //
            // Continue processing
            //
            base.OnPageEncountered(url, query, fragment);
        }

        protected override void OnRedirectPageLoaded(Uri url, IDictionary<string, string> query, IDictionary<string, string> fragment)
        {
            //
            // Look for the access_token
            //
            if (fragment.ContainsKey("access_token"))
            {
                //
                // We found an access_token
                //
                OnRetrievedAccountProperties(fragment);
            }
            else if (!IsImplicit)
            {
                //
                // Look for the code
                //
                if (query.ContainsKey("code"))
                {
                    var code = query["code"];
                    RequestAccessTokenAsync(code).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            OnError(task.Exception);
                        }
                        else
                        {
                            OnRetrievedAccountProperties(task.Result);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    OnError("Expected code in response, but did not receive one.");
                }
            }
            else
            {
                OnError("Expected access_token in response, but did not receive one.");
            }
        }

        Task<IDictionary<string, string>> RequestAccessTokenAsync(string code)
        {
            var queryValues = new Dictionary<string, string> {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", RedirectUrl.AbsoluteUri },
            { "client_id", clientId }
        };
            if (!string.IsNullOrEmpty(clientSecret))
            {
                queryValues["client_secret"] = clientSecret;
            }

            return RequestAccessTokenAsync(queryValues);
        }

        protected Task<IDictionary<string, string>> RequestAccessTokenAsync(IDictionary<string, string> queryValues)
        {
            var query = queryValues.FormEncode();

            var req = WebRequest.Create(accessTokenUrl);
            req.Method = "POST";
            var body = Encoding.UTF8.GetBytes(query);
            req.ContentLength = body.Length;
            req.ContentType = "application/x-www-form-urlencoded";
            using (var s = req.GetRequestStream())
            {
                s.Write(body, 0, body.Length);
            }
            return req.GetResponseAsync().ContinueWith(task =>
            {
                var text = task.Result.GetResponseText();

                // Parse the response
                var data = text.Contains("{") ? WebEx.JsonDecode(text) : WebEx.FormDecode(text);

                if (data.ContainsKey("error"))
                {
                    throw new AuthException("Error authenticating: " + data["error"]);
                }
                if (data.ContainsKey("access_token"))
                {
                    return data;
                }
                throw new AuthException("Expected access_token in access token response, but did not receive one.");
            });
        }

        protected virtual void OnRetrievedAccountProperties(IDictionary<string, string> accountProperties)
        {
            //
            // Now we just need a username for the account
            //
            if (getUsernameAsync != null)
            {
                getUsernameAsync(accountProperties).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        OnError(task.Exception);
                    }
                    else
                    {
                        OnSucceeded(task.Result, accountProperties);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                OnSucceeded("", accountProperties);
            }
        }

    }
}