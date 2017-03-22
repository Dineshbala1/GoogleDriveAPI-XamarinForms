using System;
using System.ComponentModel;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

namespace XamarinFormsGoogleDriveAPI.ViewModels
{
    public class GDriveFileContentViewModel: INotifyPropertyChanged
    {
        private string fileID;
        private FileDataModel realtimeData;

        public IDriveServiceManager DriveServiceManager { get; set; }

        public FileDataModel RealTimeData
        {
            get
            {
                return realtimeData;
            }
            set
            {
                realtimeData = value;
                RaisPropertyChanged("RealTimeData");
            }
        }

        public GDriveFileContentViewModel(string fileId)
        {
            fileID = fileId;
            MessagingCenter.Subscribe<FileDataModel>(this, "RealtimeData", (sender) =>
            {
                RealTimeData = sender;
                UpdateFile(fileID);
            });
            DriveServiceManager = DependencyService.Get<IDriveServiceManager>();
        }

        private void UpdateFile(string fileId)
        {
            //TODO: Not working need to check and fix
            //TODO: Fixing this will fix realtime udpates create new record.
            DriveServiceManager.UploadFileAsync(fileId, realtimeData);
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
