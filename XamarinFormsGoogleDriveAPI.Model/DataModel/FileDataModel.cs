using System;

namespace XamarinFormsGoogleDriveAPI.Model.DataModel
{
    public class FileDataModel
    {
        public string FileId { get; set; }
        public string Title { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string Owner { get; set; }
        public string SharedWith { get; set; }
    }
}
