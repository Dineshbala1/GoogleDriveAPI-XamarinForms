using System;

namespace XamarinFormsGoogleDriveAPI.Model.BusinessModel
{
    public class FileModel
    {
        public string FileId { get; set; }
        public string Title { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string Owner { get; set; }
        public string SharedWith { get; set; }

        public string LastModified
        {
            get { return ModifiedBy + " " + ModifiedTime.Value.ToString("h:mm:ss tt"); }
        }
    }
}
