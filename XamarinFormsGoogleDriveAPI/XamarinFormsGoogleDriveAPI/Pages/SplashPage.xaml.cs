using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Connectivity;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

namespace XamarinFormsGoogleDriveAPI.Pages
{
    public partial class SplashPage : ContentPage
    {
        private bool isLoaded;

        public SplashPage()
        {
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            if (isLoaded)
                return;
            //if (!CrossConnectivity.Current.IsConnected)
            //    return;
            isLoaded = true;
            if (!DependencyService.Get<ICachedUserData>().GetCachedUser())
            {
                Application.Current.MainPage = homePageNavigation();
            }
            else
            {
                Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            }
            MessagingCenter.Subscribe<string>(this, "Authenticated", (sender) =>
            {
                Application.Current.MainPage = homePageNavigation();
                MessagingCenter.Unsubscribe<string>(this, "Authenticated");
            });
        }

        private NavigationPage homePageNavigation()
        {
            var homePage = new HomePage();
            var navpage = new NavigationPage(homePage)
            {
                Title = "GDrive App",
                Icon = "icon.png",
                BarBackgroundColor = Color.FromHex("#4CAF50"),
                BarTextColor = Color.White,
            };
            return navpage;
        }
    }
}
