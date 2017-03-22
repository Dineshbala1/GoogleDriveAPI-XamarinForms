using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XamarinFormsGoogleDriveAPI.Model;
using XamarinFormsGoogleDriveAPI.Model.DataModel;

namespace XamarinFormsGoogleDriveAPI.Services.Service.Interface
{
    public interface IDriveService
    {
        Task<IList<FileDataModel>> GetFileDetailsAsync();
        Task<FileDataModel> SaveFileAsync(string fileId, string fileName);
        Task<bool> UploadFileAsync(string fileId, FileDataModel gannterModel);
    }
}
