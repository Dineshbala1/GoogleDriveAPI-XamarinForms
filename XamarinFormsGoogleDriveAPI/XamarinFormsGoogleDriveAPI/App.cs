using System;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Pages;

namespace XamarinFormsGoogleDriveAPI
{
    /// <summary>
    /// App class initializes the Xamarin.Auth component using the custom renderer 
    /// we are trying to authenticate the user against google domain and return the access token
    /// which we can try to use with the google drive api.
    /// </summary>
    public class App : Application
    {
        public static bool inAuthentication;
        static NavigationPage navigationPage;

        #region Properties

        public static Action SuccessfulLoginAction
        {
            get { return () => navigationPage?.Navigation?.PopModalAsync(); }
        }

        #endregion

        public App()
        {
            MainPage = GetMainPage();
        }

        public Page GetMainPage()
        {
            var navpage = new NavigationPage(new SplashPage())
            {
                Title = "DriveDemo App",
                Icon = "icon.png",
                BarBackgroundColor = Color.FromHex("#4CAF50"),
                BarTextColor = Color.White,
            };
            return navpage;
        }

        public void SaveToken(string token)
        {
            // Broadcast a message that authentication was successful
            MessagingCenter.Send<App>(this, "Authenticated");
            inAuthentication = false;
            SuccessfulLoginAction.Invoke();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
