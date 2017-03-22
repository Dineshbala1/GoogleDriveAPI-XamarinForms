using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Apis.Auth.OAuth2.Responses;
using Xamarin.Auth;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Droid.DependencyService;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

[assembly: Dependency(typeof(TokenIssuer))]
namespace XamarinFormsGoogleDriveAPI.Droid.DependencyService
{
    public class TokenIssuer : ITokenIssuer<TokenResponse>
    {
        private Account userAccount;

        public Account UserAccount
        {
            get
            {
                if (userAccount == null)
                {
                    userAccount = AccountStore.Create(Forms.Context).FindAccountsForService("Google").FirstOrDefault();
                }
                return userAccount;
            }
        }

        public TokenIssuer()
        {
           
        }

        public TokenResponse GetToken()
        {
            TokenResponse token = null;
            try
            {
                token = new TokenResponse()
                {
                    AccessToken = UserAccount.Properties["access_token"],
                    ExpiresInSeconds = Convert.ToInt64(UserAccount.Properties["expires_in"]),
                    IdToken = UserAccount.Properties["id_token"],
                    //IssuedUtc = Convert.ToDateTime(UserAccount.Properties["issued_utc"]),
                    RefreshToken = UserAccount.Properties["refresh_token"],
                    Scope = UserAccount.Properties["scopes"],
                    TokenType = UserAccount.Properties["token_type"],
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return token;
        }
    }
}