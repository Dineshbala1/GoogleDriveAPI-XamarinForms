using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Upload;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;


namespace XamarinFormsGoogleDriveAPI.Services.Service.Business.Service
{
    // TODO: No need for Abstract class as of now, just maintaining for future purpose.
    public abstract class BaseService
    {
        private DriveService Service { get; set; }

        protected BaseService()
        {
            InitializeDriveService();
        }

        /// <summary>
        /// Method to intialize the DriveService from GoogleDrive API.
        /// Using UserCredential (OAuth2.0) or Service Account credentails
        /// TODO: Both options not available on a app , had to comment out either one of them to run the app.Need to fix it.
        /// </summary>
        private void InitializeDriveService()
        {
            if (Service != null)
                return;
            //TODO: Service Account Credentials are commented as the Certificate needs to be used manually.
            //var credential = DependencyService.Get<IServiceCredential>().Credential;
            var googleFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = Constants.ClientId,
                    ClientSecret = Constants.ClientSecret,
                }
            });

            var tokenResponse = DependencyService.Get<ITokenIssuer<TokenResponse>>().GetToken();

            var userCredentials = new UserCredential(googleFlow, "", tokenResponse);
            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = Constants.ApplicationName,
            });
        }

        /// <summary>
        /// Method to Get the files from the Google Drive using the GoogleDrive API
        /// </summary>
        /// <param name="filesCount">Number of files to be fetched</param>
        /// <returns></returns>
        protected async Task<IList<File>> GetFileListAsync(int filesCount)
        {
            var driveFiles = new List<File>();
            try
            {
                var listRequest = Service.Files.List();
                listRequest.MaxResults = filesCount;
                var listRequestResult = await listRequest.ExecuteAsync().ConfigureAwait(false);
                var files = listRequestResult.Items.Where(row => row.MimeType.Equals("text/GDriveproject")).ToList();
                if (files == null || files?.Count <= 0)
                    return null;
                driveFiles = files.ToList();
            }
            catch (Exception)
            {

            }

            return driveFiles;
        }

        /// <summary>
        /// Method to deserialize the GDrive file content to Business models.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="fileId">Id of the file to be downloaded</param>
        /// <param name="fileName">Name of the file to be downloaded</param>
        /// <returns></returns>
        protected async Task<T> DeserializeGDriveContent<T>(string fileId, string fileName)
        {
            var rootObject = default(T);
            try
            {
                var request = Service.Realtime.Get(fileId);
                using (var memoryStream = new MemoryStream())
                {
                    request.MediaDownloader.ProgressChanged += OnMediaDownloaderProgressChanged;
                    await request.DownloadAsync(memoryStream).ConfigureAwait(false);
                    memoryStream.Position = 0;
                    var streamReader = new StreamReader(memoryStream);
                    rootObject = JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
                }
            }
            catch (Exception)
            {

            }
            return rootObject;
        }

        /// <summary>
        /// Serialize the GDriveModel to GDrive file content for saving changes.
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="fileId">Id for the file to be updated</param>
        /// <param name="GDriveModel">Content for the file to be updated</param>
        /// <returns></returns>
        protected async Task<bool> SerializeGDriveModel<T>(string fileId, T GDriveModel)
        {
            var updateSuccessful = false;
            try
            {
                var jsonString = JsonConvert.SerializeObject(GDriveModel, Formatting.None);
                var byteArray = Encoding.UTF8.GetBytes(jsonString);
                using (var stream = new MemoryStream(byteArray))
                {
                    var request = Service.Realtime.Update(fileId, stream, "text/GDriveproject");
                    request.ProgressChanged += OnRequestProgressChanged;
                    var progress = await request.UploadAsync().ConfigureAwait(false);
                    updateSuccessful = progress.Status == UploadStatus.Completed;
                }
            }
            catch (Exception)
            {

            }
            return updateSuccessful;
        }

        private void OnMediaDownloaderProgressChanged(IDownloadProgress obj)
        {
            string content;
            switch (obj.Status)
            {
                case DownloadStatus.NotStarted:
                    content = "Not Started";
                    break;
                case DownloadStatus.Downloading:
                    content = "downloading the file...";
                    break;
                case DownloadStatus.Failed:
                    content = "download failed";
                    break;
                case DownloadStatus.Completed:
                    content = "download completed";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            MessagingCenter.Send(new DownloadStatusModel() { DownloadStatus = content }, "DownloadStatus");
        }

        private void OnRequestProgressChanged(Google.Apis.Upload.IUploadProgress obj)
        {
            string content;
            switch (obj.Status)
            {
                case UploadStatus.NotStarted:
                    content = "Not Started";
                    break;
                case UploadStatus.Starting:
                    content = "Upload Starting";
                    break;
                case UploadStatus.Completed:
                    content = "Upload completed";
                    break;
                case UploadStatus.Uploading:
                    content = "Uploading the file...";
                    break;
                case UploadStatus.Failed:
                    content = "Upload failed";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            MessagingCenter.Send(new DownloadStatusModel() { DownloadStatus = content }, "UploadStatus");
        }
    }
}
