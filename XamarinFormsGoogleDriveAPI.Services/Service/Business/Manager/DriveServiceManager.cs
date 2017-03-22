using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinFormsGoogleDriveAPI.Model.BusinessModel;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services.Service.Business.Manager;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;

[assembly: Dependency(typeof(DriveServiceManager))]
namespace XamarinFormsGoogleDriveAPI.Services.Service.Business.Manager
{
    public class DriveServiceManager : IDriveServiceManager
    {
        public IDriveService DriveService { get; set; }

        public DriveServiceManager()
        {
            DriveService = DependencyService.Get<IDriveService>();
        }

        public async Task<IList<FileModel>> GetFilesList()
        {
            IList<FileModel> fileModel = null;
            var dataModel = await DriveService.GetFileDetailsAsync();
            if (dataModel != null)
            {
                fileModel =
                    dataModel.Select(
                            row =>
                                new FileModel
                                {
                                    FileId = row.FileId,
                                    ModifiedBy = row.ModifiedBy,
                                    Owner = row.Owner,
                                    Title = row.Title,
                                    ModifiedTime = row.ModifiedTime
                                })
                        .ToList();
            }
            return fileModel;
        }

        public async Task<FileDataModel> SaveFileAsync(string fileId, string fileName)
        {
            var rootObject = await DriveService.SaveFileAsync(fileId, fileName);
            return rootObject;
        }

        public async Task<bool> UploadFileAsync(string fileId, FileDataModel GDriveModel)
        {
            return await DriveService.UploadFileAsync(fileId, GDriveModel);
        }
    }
}
