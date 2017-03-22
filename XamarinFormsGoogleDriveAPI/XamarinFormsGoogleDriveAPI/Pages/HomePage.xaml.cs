using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;
using XamarinFormsGoogleDriveAPI.ViewModels;

namespace XamarinFormsGoogleDriveAPI.Pages
{
    public partial class HomePage : ContentPage
    {
        bool alreadyLoaded;
        public GDriveFileViewModel GfViewModel { get; set; }

        public HomePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            if (alreadyLoaded)
            {
                //GfViewModel??.SelectedFile = null;
                return;
            }
            InitiliazeDataContext();
            //if (!DependencyService.Get<ICachedUserData>().GetCachedUser())
            //{
            //    InitiliazeDataContext();
            //}
            //else
            //{
            //    Navigation.PushAsync(new LoginPage());
            //    MessagingCenter.Subscribe<string>(this, "Authenticated", (sender) =>
            //    {
            //        InitiliazeDataContext();
            //        MessagingCenter.Unsubscribe<string>(this, "Authenticated");
            //    });
            //}
            MessagingCenter.Subscribe<string>(this, "Navigate", async (sender) =>
            {
                await Navigation.PushAsync(new GDriveFileContentPage());
            });
            MessagingCenter.Subscribe<string>(this, "CreateNew", sender =>
            {
                CreateNewProject();
            });

            alreadyLoaded = true;
        }

        private void InitiliazeDataContext()
        {
            GfViewModel = new GDriveFileViewModel();
            GfViewModel.FetchFileList();
            BindingContext = GfViewModel;
        }

        private void CreateNewProject()
        {
            Navigation.PushPopupAsync(new CreateNewProject(), true);
        }
    }
}
