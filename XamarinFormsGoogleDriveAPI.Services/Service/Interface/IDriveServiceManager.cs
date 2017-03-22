using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.Model.DataModel;

namespace XamarinFormsGoogleDriveAPI.Services.Service.Interface
{
    public interface IDriveServiceManager
    {
        Task<IList<FileModel>> GetFilesList();
        Task<FileDataModel> SaveFileAsync(string fileId, string fileName);
        Task<bool> UploadFileAsync(string fileId, FileDataModel jsonContent);
    }
}
