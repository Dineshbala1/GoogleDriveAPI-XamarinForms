using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BottomNavigationBar.XamarinForms;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.ViewModels;

namespace XamarinFormsGoogleDriveAPI.Pages
{
    public partial class GDriveFileContentPage : BottomBarPage
    {
        public GDriveFileContentPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<FileModel>(this, "SelectedFile", (sender) =>
            {
                Title = sender.Title;
            });
        }

        private void GDriveFileContentPage_OnLayoutChanged(object sender, EventArgs e)
        {
            SelectedTabColor = Color.FromHex("#4CAF50");
        }
    }
}
