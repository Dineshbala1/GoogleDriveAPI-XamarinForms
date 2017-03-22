using System;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;
using XamarinFormsGoogleDriveAPI.Model.DataModel;
using XamarinFormsGoogleDriveAPI.Services.Service.Business.Service;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(DriveServiceImpl))]

namespace XamarinFormsGoogleDriveAPI.Services.Service.Business.Service
{
    public class DriveServiceImpl : BaseService, IDriveService
    {
        /// <summary>
        /// Method to get the list of available files from the Google Drive
        /// </summary>
        /// <returns></returns>
        public async Task<IList<FileDataModel>> GetFileDetailsAsync()
        {
            var files = await GetFileListAsync(100);
            return (from file in files
                where file.ExplicitlyTrashed != null && !file.ExplicitlyTrashed.Value
                select new FileDataModel
                {
                    FileId = file.Id,
                    ModifiedBy = file.LastModifyingUser.DisplayName,
                    ModifiedTime = file.ModifiedDate.HasValue ? file.ModifiedDate.Value : DateTime.MinValue,
                    Title = file.Title,
                    Owner = file.OwnerNames.First(),
                    SharedWith = file.SharingUser?.DisplayName
                }).ToList();
        }

        /// <summary>
        /// Method to download and save the file locally in the device.
        /// </summary>
        /// <param name="fileId">Id of the file to be downloaded</param>
        /// <param name="fileName">Name of the file to be downloaded</param>
        /// <returns></returns>
        public async Task<FileDataModel> SaveFileAsync(string fileId, string fileName)
        {
            return await DeserializeGDriveContent<FileDataModel>(fileId, fileName);
        }

        /// <summary>
        /// Method to update the file in the google drive with local changes
        /// </summary>
        /// <param name="fileId">Id of the file to be updated</param>
        /// <param name="gannterModel">Model of the GDriveFile</param>
        /// <returns></returns>
        public async Task<bool> UploadFileAsync(string fileId, FileDataModel gannterModel)
        {
            return await SerializeGDriveModel(fileId, gannterModel);
        }
    }
}
