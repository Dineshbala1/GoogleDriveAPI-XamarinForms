using System.IO;

namespace XamarinFormsGoogleDriveAPI.Services.Service.Interface
{
    public interface IFile
    {
        Stream ConvertToByes(string fileContent);
    }
}
