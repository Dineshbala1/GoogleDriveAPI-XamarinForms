using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Xamarin.Auth;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Droid.DependencyService;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

[assembly: Dependency(typeof(ServiceCredentialImpl))]
namespace XamarinFormsGoogleDriveAPI.Droid.DependencyService
{
    public class ServiceCredentialImpl : IServiceCredential
    {
        private ServiceAccountCredential credential;
        public ServiceCredential Credential => credential;

        public string FileLocation => System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        public ServiceCredentialImpl()
        {
            var certificate =
                new X509Certificate2(FileLocation + "/" + "your .p12 certificate or the json to read the private key for service account credentials",
                    "notasecret", X509KeyStorageFlags.Exportable);
            var cache = AccountStore.Create().FindAccountsForService("Google").FirstOrDefault();
            credential =
                new ServiceAccountCredential(new ServiceAccountCredential.Initializer(
                    "your client-id here")
                {
                    //Scopes = Constants.Scopes,
                    User = cache.Properties["Email"],
                }.FromCertificate(certificate));
        }

        public string SaveFile(string fileName, MemoryStream stream)
        {
            var filePath = FileLocation + "/" + fileName;
            using (var targetStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.WriteTo(targetStream);
                targetStream.Close();
            }
            MessagingCenter.Send(new DownloadStatusModel() {DownloadStatus = "File Created in the local Memory"},
                "DownloadStatus");
            return filePath;
        }
    }
}
