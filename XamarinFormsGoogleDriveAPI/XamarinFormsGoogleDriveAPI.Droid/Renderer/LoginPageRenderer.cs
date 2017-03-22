using System;
using System.Json;
using Android.App;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinFormsGoogleDriveAPI;
using XamarinFormsGoogleDriveAPI.Droid.Renderer;
using XamarinFormsGoogleDriveAPI.Pages;
using XamarinFormsGoogleDriveAPI.Services;

[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]

namespace XamarinFormsGoogleDriveAPI.Droid.Renderer
{
    public class LoginPageRenderer : PageRenderer
    {
        string userJson;

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (App.inAuthentication) return;
            App.inAuthentication = true;
            // this is a ViewGroup - so should be able to load an AXML file and FindView<>
            var activity = Forms.Context as Activity;

            var auth = new RefreshOAuth2Authenticator(
                Constants.ClientId,
                Constants.ClientSecret,
                Constants.Scopes,
                new Uri(Constants.AuthorizationUri),
                new Uri(Constants.RedirectUri),
                new Uri(Constants.AccessTokenUri));

            auth.AllowCancel = true;

            auth.Completed += async (sender, eventArgs) =>
            {
                if (eventArgs.IsAuthenticated)
                {
                    App.SuccessfulLoginAction.Invoke();
                }

                var request = new OAuth2Request("GET", new Uri("https://www.googleapis.com/oauth2/v2/userinfo"), null,
                    eventArgs.Account);
                var response = await request.GetResponseAsync();

                if (response != null)
                {
                    userJson = response.GetResponseText();
                }
                StoringDataIntoCache(userJson, eventArgs.Account);
            };

            //TODO: Fix the output error
            //auth.ShowErrors = false;

            activity.StartActivity(auth.GetUI(activity));
        }

        /// <summary>
        /// Method to cache the authorized user data
        /// </summary>
        /// <param name="userData"></param>
        void StoringDataIntoCache(string userData, Account accountdata)
        {
            var data = JsonValue.Parse(userData);

            //var account = new Account();
            accountdata.Properties.Add("User", data["name"]);
            accountdata.Properties.Add("Email", data["email"]);
            accountdata.Properties.Add("scopes", Constants.Scopes);
            (Forms.Context as Activity)?.RunOnUiThread(() =>
            {
                AccountStore.Create(Forms.Context).Save(accountdata, "Google");
            });

            MessagingCenter.Send("Authentication Successful", "Authenticated");
        }
    }
}