using System;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;
using XamarinFormsGoogleDriveAPI.Droid.DependencyService;
using Xamarin.Forms;
using System.IO;
using System.Text;

[assembly: Dependency(typeof(FileImpl))]
namespace XamarinFormsGoogleDriveAPI.Droid.DependencyService
{
    public class FileImpl : IFile
    {
        public Stream ConvertToByes(string fileContent)
        {
            return null;
        }
    }
}