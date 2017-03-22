
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.ViewModels;

namespace XamarinFormsGoogleDriveAPI.Pages
{
    public class FilePage : ContentPage
    {
        public GDriveFileContentViewModel GfcViewModel { get; set; }

        public FilePage()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("RealTimeData.appId", BindingMode.TwoWay));
            Content = new ScrollView()
            {
                Content = label
            };

            MessagingCenter.Subscribe<string>(this, "FileContent", (sender) =>
            {
                ((Label) ((ScrollView) Content).Content).SetBinding(Label.TextProperty,
                    new Binding("RealTimeData.appId", BindingMode.TwoWay));
            });

            MessagingCenter.Subscribe<FileModel>(this, "SelectedFile", (sender) =>
            {
                GfcViewModel = new GDriveFileContentViewModel(sender.FileId);
                BindingContext = GfcViewModel;
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<string>(this, "FileContent");
        }
    }
}
