using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;
using XamarinFormsGoogleDriveAPI.ViewModels;

namespace XamarinFormsGoogleDriveAPI
{
    public class ProfilePage : BaseContentPage
    {
        bool alreadyLoaded;
        StackLayout stackLayout = new StackLayout();

        public GantterFileViewModel GFViewModel { get; set; }

        public ProfilePage()
        {
            Title = "File Explorer";
            GFViewModel = new GantterFileViewModel();
            BindingContext = GFViewModel;
            InitializeUIView();
            Content = stackLayout;
            InitMessageCenter();
        }

        private void InitializeUIView()
        {

            var activityIndicator = new ActivityIndicator();
            activityIndicator.SetBinding(IsVisibleProperty, new Binding("IsBusy", BindingMode.TwoWay, null, null, null, null));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsBusy", BindingMode.TwoWay, null, null, null, null));
            activityIndicator.HorizontalOptions = LayoutOptions.Center;
            activityIndicator.VerticalOptions = LayoutOptions.Center;
            stackLayout.Children.Add(activityIndicator);
            var listview = new ListView(ListViewCachingStrategy.RecycleElement) { ItemTemplate = new DataTemplate(listTemplate) };
            listview.SetBinding(ListView.ItemsSourceProperty, new Binding("FileList", BindingMode.TwoWay, null, null, null, null));
            listview.RowHeight = 120;
            listview.IsPullToRefreshEnabled = true;
            listview.ItemSelected += Listview_ItemSelected;
            stackLayout.Children.Add(listview);
        }

        private void InitMessageCenter()
        {

            MessagingCenter.Subscribe<App>(this, "Authenticated", (sender) =>
            {
                stackLayout.Children.Add(new Label()
                {
                    Text = "Profile Page",
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                });
            });

            MessagingCenter.Subscribe<DownloadStatusModel>(this, "DownloadStatus", (sender) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Downloading Status", sender.DownloadStatus, "Close");
                    Task.WaitAll();
                });
            });
        }

        protected override void OnAppearing()
        {
            if (alreadyLoaded)
                return;
            if (DependencyService.Get<ICachedUserData>().GetCachedUser())
            {
                Navigation.PushModalAsync(new LoginPage());
            }
            else
            {
                GFViewModel.FetchFileList();
            }
            alreadyLoaded = true;
        }

        private void Listview_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            DownloadFile((e.SelectedItem as FileModel).FileId, (e.SelectedItem as FileModel).Title);
        }

        private async void DownloadFile(string fileId, string fileName)
        {
            await Navigation.PushAsync(new FilePage(fileName));
            GFViewModel.SaveFileAsync(fileId, "user" + fileId + ".json");
        }

        private object listTemplate()
        {
            var frame = new Frame();
            var stackLayout = new StackLayout();
            stackLayout.Orientation = StackOrientation.Vertical;
            var fileTitle = new Label { FontAttributes = FontAttributes.Bold };
            var fileId = new Label();
            var owner = new Label { HorizontalTextAlignment = TextAlignment.End };

            fileTitle.SetBinding(Label.TextProperty, "Title");
            fileId.SetBinding(Label.TextProperty, "FileId");
            owner.SetBinding(Label.TextProperty, "Owner");

            stackLayout.Children.Add(fileTitle);
            stackLayout.Children.Add(fileId);
            stackLayout.Children.Add(owner);

            frame.Content = stackLayout;
            frame.HasShadow = true;
            frame.OutlineColor = Color.Purple;
            frame.BackgroundColor = Color.Transparent;
            frame.Margin = new Thickness(4);

            var cell = new ViewCell { View = frame };
            cell.ContextActions.Add(new MenuItem() { Text = "Hello", });
            return cell;
        }
    }
}

