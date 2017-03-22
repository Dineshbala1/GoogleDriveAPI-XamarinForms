using Google.Apis.Auth.OAuth2;
using System.IO;

namespace XamarinFormsGoogleDriveAPI.Services.Service.Interface
{
    public interface IServiceCredential
    {
        ServiceCredential Credential { get; }

        string SaveFile(string fileName, MemoryStream stream);
    }
}
