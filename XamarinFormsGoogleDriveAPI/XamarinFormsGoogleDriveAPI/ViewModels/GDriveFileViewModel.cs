using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.Pages;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

namespace XamarinFormsGoogleDriveAPI.ViewModels
{
    public class GDriveFileViewModel : INotifyPropertyChanged
    {
        public IDriveServiceManager DriveServiceManager { get; set; }

        private IList<FileModel> fileList;
        private FileModel selectedFile;

        public IList<FileModel> FileList
        {
            get { return fileList; }
            internal set
            {
                fileList = value;
                GroupedFileModel = fileList?.GroupBy(row => row.LastModified).ToList();
                RaisPropertyChanged("FileList");
            }
        }

        private IList<IGrouping<string, FileModel>> groupFileModel;

        public IList<IGrouping<string, FileModel>> GroupedFileModel
        {
            get { return groupFileModel; }
            set
            {
                groupFileModel = value;
                RaisPropertyChanged("GroupedFileModel");
            }
        }

        private ICommand showCommand;

        public ICommand ShowPopUpCommand
        {
            get { return showCommand; }
            set { showCommand = value; }
        }

        public FileModel SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                RaisPropertyChanged("SelectedFile");
                DownloadFile();
            }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                RaisPropertyChanged("IsBusy");
            }
        }

        public GDriveFileViewModel()
        {
            DriveServiceManager = DependencyService.Get<IDriveServiceManager>();
            IsBusy = true;
            ShowPopUpCommand = new Command(ShowPopUp);
        }

        public async void FetchFileList()
        {
            FileList = await DriveServiceManager.GetFilesList();
            IsBusy = false;
        }

        public async void SaveFileAsync(string fileId, string fileName)
        {
            var realtimeModel = await DriveServiceManager.SaveFileAsync(fileId, fileName);
            MessagingCenter.Send(realtimeModel, "RealtimeData");
        }

        private void DownloadFile()
        {
            if (SelectedFile != null)
            {
                MessagingCenter.Send("SelectedItemChanged", "Navigate");
                MessagingCenter.Send(SelectedFile, "SelectedFile");
                SaveFileAsync(SelectedFile.FileId, "File" + SelectedFile.FileId + ".json");
            }
        }

        private void ShowPopUp()
        {
            MessagingCenter.Send("CreateNewProject", "CreateNew");
        }

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
