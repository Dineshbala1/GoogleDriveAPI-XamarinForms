using System;
using System.Json;
using Xamarin.Auth;
using Xamarin.Forms;
#if __IOS__
using Xamarin.Forms.Platform.iOS;
#else
using Xamarin.Forms.Platform.Android;
using Android.App;
#endif
using XamarinFormsGoogleDriveAPI;
using XamarinFormsGoogleDriveAPI.Droid.Renderer;
using XamarinFormsGoogleDriveAPI.Pages;
using XamarinFormsGoogleDriveAPI.Services;

//Xamarin.Auth Renderer it was made of cross platform
//TODO: Looks needs to be cleaned up, needs to check on Xamarin.Auth updates for cross platform support.
[assembly: ExportRenderer(typeof(LoginPage), typeof(LoginPageRenderer))]

namespace XamarinFormsGoogleDriveAPI.Droid.Renderer
{
    public class LoginPageRenderer : PageRenderer
    {
        string userJson;
#if __IOS__
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
#else
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
#endif
        {
            base.OnElementChanged(e);
#if !__IOS__
            if (App.inAuthentication) return;
            App.inAuthentication = true;
            // this is a ViewGroup - so should be able to load an AXML file and FindView<>

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
            var activity = Forms.Context as Activity;
            activity.StartActivity(auth.GetUI(activity));
#endif
        }

#if __IOS__
        public override void ViewDidAppear(bool animated)
        {
            if (App.inAuthentication)
                return;
            base.ViewDidAppear(animated);
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
                    App.inAuthentication = true;
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
            auth.ShowErrors = false;
            PresentViewController(auth.GetUI(), true, null);
        }
#endif

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
#if __IOS__
            AccountStore.Create().Save(accountdata, "Google");
#else
            (Forms.Context as Activity)?.RunOnUiThread(() =>
            {
                AccountStore.Create(Forms.Context).Save(accountdata, "Google");
            });
#endif
            MessagingCenter.Send("Authentication Successful", "Authenticated");
        }
    }
}