using System.Linq;
using XamarinFormsGoogleDriveAPI.Services.Service.Interface;
using Xamarin.Auth;
using XamarinFormsGoogleDriveAPI.Droid.DependencyService;

[assembly: Xamarin.Forms.Dependency(typeof(CacheUserData))]
namespace XamarinFormsGoogleDriveAPI.Droid.DependencyService
{
    public class CacheUserData : ICachedUserData
    {
        public bool GetCachedUser()
        {
            var cache = AccountStore.Create().FindAccountsForService("Google").FirstOrDefault();
            return cache == null;
        }
    }
}